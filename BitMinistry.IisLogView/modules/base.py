from abc import ABC, abstractmethod
import streamlit as st
import pandas as pd

from utils import sql as sql_utils


class Module(ABC):
    """Base for all modules. Subclasses implement render()."""
    name: str

    @abstractmethod
    def render(self):
        """Render the full module UI (main area). Sidebar is optional."""
        pass


class ImporterModule(Module):
    """For import-style flows: sidebar, form, run(), then Results/Errors tabs."""
    @abstractmethod
    def render_sidebar(self):
        """Return defaults dict for run()."""
        pass

    @abstractmethod
    def run(self, files, defaults):
        """Process input. Return (results_list, errors_list)."""
        pass

    def render(self):
        st.title(f"📥 {self.name}")
        defaults = self.render_sidebar()
        with st.form(f"{self.name}_form"):
            submitted = st.form_submit_button("Run", type="primary")
        if submitted:
            with st.spinner("Running..."):
                st.session_state.results, st.session_state.errors = self.run(None, defaults)
        tab1, tab2 = st.tabs(["Results", "Errors"])
        with tab1:
            if st.session_state.get("results"):
                st.dataframe(pd.DataFrame(st.session_state.results), use_container_width=True)
        with tab2:
            for err in st.session_state.get("errors", []):
                st.error(err)


class ViewerModule(Module):
    """For view-style: query MSSQL and show tables/graphs. No file upload or run()."""

    def run_query(self, query_str: str, params=None) -> pd.DataFrame:
        """Run a read-only query and return a DataFrame."""
        rows = sql_utils.query(query_str, params)
        return pd.DataFrame(rows) if rows else pd.DataFrame()

    def render(self):
        """Override in subclasses to show tables and graphs from MSSQL."""
        st.title(f"📊 {self.name}")
        st.info("Override render() to show your queries and charts.")
