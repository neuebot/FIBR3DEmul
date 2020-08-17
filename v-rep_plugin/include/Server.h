//
// async_tcp_echo_server.cpp
// ~~~~~~~~~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2008 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#pragma once

#include <boost/asio.hpp>
#include <boost/bind.hpp>
#include <boost/thread.hpp>

#include "Listener.h"
#include "Speaker.h"

#include <cstdlib>
#include <iostream>

using boost::asio::ip::tcp;

class SessionEcho
{
public:
	SessionEcho(boost::asio::io_service* io_service);
	tcp::socket& get_socket();

	void start();
	void handle_read(const boost::system::error_code& error, size_t bytes_transferred);
	void handle_write(const boost::system::error_code& error);

private:
	tcp::socket socket_;
	enum {
		max_length = 512
	};
	char data_[max_length];

	std::unique_ptr<Listener> m_listener;
};

class SessionSend
{
public:
	SessionSend(boost::asio::io_service* io_service);
	tcp::socket& get_socket();
	
	void deliver(int line, bool collision);
	void write();
	void handle_write(const boost::system::error_code& error);

private:
	tcp::socket socket_;
	enum { 
		max_length = 1024
	};
	char data_[max_length];

	std::shared_ptr<Speaker> m_speaker;
};

class Server
{
public:
	Server(boost::asio::io_service* io_service, short port1, short port2 );
	//tcp::acceptor& get_acceptor();

	void start_accept();
	void handle_accept_echo(const boost::system::error_code& error);
	void handle_accept_send(const boost::system::error_code& error);

	void broadcast(int line, bool collision);

	void cancel_sessions();

private:
	boost::asio::io_service* io_service_;

	tcp::acceptor acceptor_echo_;
	tcp::acceptor acceptor_send_;

	std::shared_ptr<SessionEcho> session_echo;
	std::shared_ptr<SessionSend> session_send;
};

//////////////////////////////////////////////////////////////////////

class CommunicationManager {
public:
	CommunicationManager();
	~CommunicationManager();
	std::shared_ptr<Server> get_server();

	void LaunchServer();
	void StopServer();

private:
	boost::asio::io_service ios;
	std::shared_ptr<boost::thread> bt;
	std::shared_ptr<Server> server;
};