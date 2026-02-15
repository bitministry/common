import os
import streamlit as st
import pandas as pd
from modules.base import ImporterModule
from services.iis_log import process_logs


class LogImportModule(ImporterModule):
    name = "IIS Log Import"

    def render_sidebar(self):
        """Render sidebar with log root selection and subdirectory picker."""
        log_root = st.text_input(
            "IIS Log Root Directory",
            value=r"C:\inetpub\logs\LogFiles",
            help="Root directory containing IIS log folders (W3SVC*)"
        )
        
        # Show subdirectories if root exists
        subdirs = []
        if log_root and os.path.exists(log_root):
            try:
                items = os.listdir(log_root)
                subdirs = [d for d in items if os.path.isdir(os.path.join(log_root, d))]
                subdirs.sort()
            except Exception as e:
                st.error(f"Error reading directory: {e}")
        
        selected_subdirs = []
        if subdirs:
            for s in subdirs:
                if f"subdir_{s}" not in st.session_state:
                    st.session_state[f"subdir_{s}"] = True
            hdr, b1, b2 = st.columns([3, 1, 1])
            with hdr:
                st.write("**Subdirectories:**")
            with b1:
                if st.button("All"):
                    for s in subdirs:
                        st.session_state[f"subdir_{s}"] = True
                    st.rerun()
            with b2:
                if st.button("None"):
                    for s in subdirs:
                        st.session_state[f"subdir_{s}"] = False
                    st.rerun()
            cols = st.columns(min(len(subdirs), 4))
            for i, subdir in enumerate(subdirs):
                with cols[i % len(cols)]:
                    if st.checkbox(subdir, key=f"subdir_{subdir}"):
                        selected_subdirs.append(subdir)
        else:
            st.info("No subdirectories found. Will process root directory recursively.")
            selected_subdirs = [""]  # Empty means process root
        
        return {
            "log_root": log_root,
            "selected_subdirs": selected_subdirs
        }

    def run(self, files, defaults):
        """Process IIS logs from selected directories."""
        log_root = defaults.get("log_root", r"C:\inetpub\logs\LogFiles")
        selected_subdirs = defaults.get("selected_subdirs", [])
        
        if not log_root or not os.path.exists(log_root):
            return [], [f"Log root directory does not exist: {log_root}"]
        
        results = []
        errors = []
        
        # Process each selected subdirectory (or root if empty)
        dirs_to_process = []
        if selected_subdirs and selected_subdirs != [""]:
            for subdir in selected_subdirs:
                full_path = os.path.join(log_root, subdir)
                if os.path.exists(full_path):
                    dirs_to_process.append(full_path)
        else:
            dirs_to_process = [log_root]
        
        for dir_path in dirs_to_process:
            try:
                st.write(f"Processing: {dir_path}")
                # Actual import: services/iis_log.ingest_all() reads .log files and writes to SQL Server
                process_logs(dir_path)
                results.append({
                    "Directory": dir_path,
                    "Status": "Success",
                    "Message": "Logs imported successfully"
                })
            except Exception as e:
                error_msg = f"Error processing {dir_path}: {str(e)}"
                errors.append(error_msg)
                results.append({
                    "Directory": dir_path,
                    "Status": "Error",
                    "Message": str(e)
                })
        
        return results, errors

    def render(self):
        """Override base render to use directory selection instead of file upload."""
        st.title(f"📥 {self.name}")
        defaults = self.render_sidebar()
        
        with st.form(f"{self.name}_form"):
            submitted = st.form_submit_button("Import Logs", type="primary")
        
        if submitted:
            if not defaults.get("log_root"):
                st.warning("Log root directory is required")
            else:
                with st.spinner("Importing logs..."):
                    st.session_state.results, st.session_state.errors = self.run(None, defaults)
        
        tab1, tab2 = st.tabs(["Results", "Errors"])
        with tab1:
            if st.session_state.get("results"):
                st.dataframe(pd.DataFrame(st.session_state.results), use_container_width=True)
        with tab2:
            for err in st.session_state.get("errors", []):
                st.error(err) 
