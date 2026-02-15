import streamlit as st
from modules.registry import MODULES

st.sidebar.title("IIS Log Analyzer")
selected = st.sidebar.radio("Module:", list(MODULES.keys()))
st.sidebar.divider()

MODULES[selected].render()
