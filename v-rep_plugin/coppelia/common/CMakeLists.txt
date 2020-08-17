file(GLOB src_files *.cpp)
file(GLOB stack_src_files stack/*.cpp)
set(coppelia_plugin_src ${src_files} ${stack_src_files} CACHE INTERNAL "" FORCE)
