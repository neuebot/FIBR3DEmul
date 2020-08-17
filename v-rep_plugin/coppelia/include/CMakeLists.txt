file(GLOB hdr_files *.h)
file(GLOB stack_hdr_files stack/*.h)
set(coppelia_plugin_hdr ${hdr_files} ${stack_hdr_files} CACHE INTERNAL "" FORCE)