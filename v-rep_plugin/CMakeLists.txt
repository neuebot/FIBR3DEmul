cmake_minimum_required(VERSION 2.8)
project (v_repExtFIBR3DPlugin)
# COMPILATION FLAGS

#INCLUDE DIRECTORIES AND SOURCE DIRECTORIES
include_directories(vrep/include)
include_directories(include)
include_directories(spdlog)

set(Boost_USE_STATIC_LIBS ON)
find_package(Boost 1.64.0)
if(Boost_FOUND)
	INCLUDE_DIRECTORIES(${Boost_INCLUDE_DIRS})
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

add_subdirectory(include)
add_subdirectory(vrep/common)
add_subdirectory(src)

#project source, include and resource files
set( HEADER_FILES
        ${vrep_plugin_hdr}
        ${fibr3d_hdr}
        v_repExtFIBR3D.h
)

set( SOURCE_FILES
        ${vrep_plugin_src}
        ${fibr3d_src}
        v_repExtFIBR3D.cpp
)

set(FIBR3D_SRCS
    ${HEADER_FILES}
    ${SOURCE_FILES}
)

add_library(${PROJECT_NAME} SHARED ${FIBR3D_SRCS})

#link to which libs
target_link_libraries(${PROJECT_NAME} ${Boost_LIBRARIES})# ${PCL_LIBRARIES})

