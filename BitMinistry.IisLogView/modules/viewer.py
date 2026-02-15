import streamlit as st
import pandas as pd
from modules.base import ViewerModule


class ViewerModuleImpl(ViewerModule):
    name = "Viewer"

    def render(self):
        st.title(f"📊 {self.name}")


        tab_overview, tab_agents, tab_sessions, tab_requests = st.tabs([
            "Overview",
            "By User Agent",
            "Sessions",
            "Requests"
        ])

        with tab_overview:
            df_visitors = self.run_query("""
                SELECT 
                    IsBot,
                    DeviceType,
                    COUNT(*) AS Visitors,
                    COUNT(DISTINCT IpAddress) AS UniqueIPs
                FROM IisVisitors
                GROUP BY IsBot, DeviceType
                ORDER BY Visitors DESC
            """)
            if not df_visitors.empty:
                st.subheader("Visitors (Bot vs Human, Device)")
                st.dataframe(df_visitors, use_container_width=True)
                st.bar_chart(df_visitors.set_index("DeviceType")["Visitors"])
            else:
                st.info("No visitor data yet. Run IIS Log Import first.")

            df_countries = self.run_query("""
                SELECT TOP 20
                    COALESCE(Country, '(unknown)') AS Country,
                    COUNT(*) AS Visitors
                FROM IisVisitors
                GROUP BY Country
                ORDER BY Visitors DESC
            """)
            if not df_countries.empty:
                st.subheader("Top countries")
                st.dataframe(df_countries, use_container_width=True)
                st.bar_chart(df_countries.set_index("Country")["Visitors"])

        with tab_agents:
            df_ua = self.run_query("""
                SELECT 
                    UserAgent,
                    IsBot,
                    COUNT(*) AS Visitors
                FROM IisVisitors
                GROUP BY UserAgent, IsBot
                ORDER BY Visitors DESC
            """)
            if not df_ua.empty:
                st.dataframe(df_ua, use_container_width=True)
                st.bar_chart(df_ua.set_index("UserAgent")["Visitors"])
            else:
                st.info("No data.")

        with tab_sessions:
            df_sess = self.run_query("""
                SELECT TOP 500
                    s.SessionId,
                    v.IpAddress,
                    v.UserAgent,
                    v.IsBot,
                    s.StartedUtc,
                    s.ReferrerClass,
                    s.TotalRequests,
                    s.DurationSeconds
                FROM IisSessions s
                JOIN IisVisitors v ON v.VisitorId = s.VisitorId
                ORDER BY s.StartedUtc DESC
            """)
            if not df_sess.empty:
                st.dataframe(df_sess, use_container_width=True)
            else:
                st.info("No sessions.")

        with tab_requests:
            df_req = self.run_query("""
                SELECT TOP 500
                    r.RequestId,
                    r.RequestTimeUtc,
                    r.Method,
                    r.UrlPath,
                    r.StatusCode,
                    r.TimeOnPageSec
                FROM IisRequests r
                ORDER BY r.RequestTimeUtc DESC
            """)
            if not df_req.empty:
                st.dataframe(df_req, use_container_width=True)
            else:
                st.info("No requests.")
