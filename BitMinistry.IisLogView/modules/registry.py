from modules.log_import import LogImportModule
from modules.viewer import ViewerModuleImpl


MODULES = {
    LogImportModule.name: LogImportModule(),
    ViewerModuleImpl.name: ViewerModuleImpl(),
}
