cmake_minimum_required(VERSION 2.8)
project (simExtFIBR3D)
# COMPILATION FLAGS

#COPPELIA
if(NOT LIBPLUGIN_DIR)
    if(DEFINED ENV{COPPELIASIM_ROOT_DIR})
        set(LIBPLUGIN_DIR $ENV{COPPELIASIM_ROOT_DIR}/programming/libPlugin)
    else()
        set(LIBPLUGIN_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../libPlugin)
    endif()
endif()
set(CMAKE_MODULE_PATH
    ${CMAKE_MODULE_PATH}
    ${CMAKE_CURRENT_SOURCE_DIR}/cmake/modules
    ${LIBPLUGIN_DIR}/cmake)
find_package(CoppeliaSim 4.0.0.4 REQUIRED)

#BOOST
set(Boost_USE_STATIC_LIBS ON)
find_package(Boost)
if(Boost_FOUND)
    INCLUDE_DIRECTORIES(${Boost_INCLUDE_DIR})
	LINK_DIRECTORIES(${Boost_LIBRARY_DIRS})	
endif()

# PCL - PointCloud to Mesh Operation
#find_package(PCL 1.7 REQUIRED)
#if(PCL_FOUND)
#	include_directories(${PCL_INCLUDE_DIRS})
#	link_directories(${PCL_LIBRARY_DIRS})
#	add_definitions(${PCL_DEFINITIONS})
#endif()

#include_directories(EIGEN3_INCLUDE_DIR)

#INCLUDE DIRECTORIES AND SOURCE DIRECTORIES
include_directories(${COPPELIASIM_INCLUDE_DIR})
include_directories(${CMAKE_CURRENT_BINARY_DIR}/generated)
include_directories(${CMAKE_CURRENT_SOURCE_DIR}/external)

include_directories(coppelia/include)
include_directories(coppelia/include/stack)
include_directories(include)
include_directories(spdlog)

add_subdirectory(include)
add_subdirectory(coppelia/common)
add_subdirectory(src)


coppeliasim_generate_stubs(${CMAKE_CURRENT_BINARY_DIR}/generated XML_FILE
    ${CMAKE_CURRENT_SOURCE_DIR}/callbacks.xml
    LUA_FILE
    ${CMAKE_CURRENT_SOURCE_DIR}/simExtPluginSkeletonNG.lua)

#project source, include and resource files
set( HEADER_FILES
        ${coppelia_plugin_hdr}
        ${fibr3d_hdr}
        simExtFIBR3D.h
)

set( SOURCE_FILES
        ${coppelia_plugin_src}
        ${fibr3d_src}
        simExtFIBR3D.cpp
)

set(FIBR3D_SRCS
    ${HEADER_FILES}
    ${SOURCE_FILES}
)

add_library(${PROJECT_NAME} SHARED ${FIBR3D_SRCS})

#link to which libs
target_link_libraries(${PROJECT_NAME} ${Boost_LIBRARIES} ${COPPELIASIM_LIBRARIES})# ${PCL_LIBRARIES})
install(TARGETS ${PROJECT_NAME} DESTINATION $ENV{COPPELIASIM_ROOT_DIR})

