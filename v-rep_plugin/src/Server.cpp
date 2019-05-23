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

#include "Server.h"
#include "../v_repExtFIBR3D.h"

using boost::asio::ip::tcp;

/****************************************************************************************
										ECHO
****************************************************************************************/

SessionEcho::SessionEcho(boost::asio::io_service * io_service) : socket_(*io_service)
{
	m_listener = std::make_unique<Listener>();
}

tcp::socket& SessionEcho::get_socket()
{
	return socket_;
}

//Imediately asks to read and then goes to ext_handle when finished
void SessionEcho::start()
{
	socket_.async_read_some(boost::asio::buffer(data_, max_length),
		boost::bind(&SessionEcho::handle_read, this,
			boost::asio::placeholders::error,
			boost::asio::placeholders::bytes_transferred));
}

void SessionEcho::handle_read(const boost::system::error_code & error, size_t bytes_transferred)
{
	if (!error)
	{
		// Send echo reply
		//Client requires echo reply
		boost::asio::async_write(socket_,
			boost::asio::buffer(data_, bytes_transferred),
			boost::bind(&SessionEcho::handle_write, this,
				boost::asio::placeholders::error));

		try
		{
			// Create string from data buffer
			std::string msg(data_, data_ + bytes_transferred);
			
			// Interpret string
			m_listener->Interpret(msg);
		}
		catch (const std::exception &e)
		{
			v_repExtPQBarMsgCommunication(e.what());
		}
	}
	else
	{
		//SIGNAL SERVER TO DELETE SESSION
		v_repExtPQBoxMsgCommunication("No connection with client.");

		v_repExtPQBarMsgCommunication("Disconnected from client.");
	}
}

void SessionEcho::handle_write(const boost::system::error_code & error)
{
	socket_.async_read_some(boost::asio::buffer(data_, max_length),
		boost::bind(&SessionEcho::handle_read, this,
			boost::asio::placeholders::error,
			boost::asio::placeholders::bytes_transferred));
}

/****************************************************************************************
										SEND
****************************************************************************************/

SessionSend::SessionSend(boost::asio::io_service * io_service) : socket_(*io_service)
{
	m_speaker = std::make_unique<Speaker>();
}

tcp::socket& SessionSend::get_socket()
{
	return socket_;
}

void SessionSend::deliver(int line, bool collision)
{
	//Write new message
	m_speaker->Formulate(line, collision);

	write();
}

// Imediately writes buffer
void SessionSend::write()
{
	if (m_speaker->NumberMessages() > 0)
	{
		//Get message from queue
		std::string msg = m_speaker->ReadMessage();
		//Put message in line buffer
		msg.copy(data_, msg.length());

		//Client requires echo reply
		boost::asio::async_write(socket_,
			boost::asio::buffer(data_, msg.length()), 
			boost::asio::transfer_exactly(msg.length()),
			boost::bind(&SessionSend::handle_write, this,
				boost::asio::placeholders::error));
	}
}

void SessionSend::handle_write(const boost::system::error_code& error)
{
	if (!error)
	{
		// If there is more numbers in the list
		if (m_speaker->NumberMessages() > 0)
		{
			//Get message from queue
			std::string msg = m_speaker->ReadMessage();
			//Put message in line buffer
			msg.copy(data_, msg.length());

			//Client requires echo reply
			boost::asio::async_write(socket_,
				boost::asio::buffer(data_, msg.length()),
				boost::asio::transfer_exactly(msg.length()),
				boost::bind(&SessionSend::handle_write, this,
					boost::asio::placeholders::error));
		}
	}
	else
	{
		//SIGNAL SERVER TO DELETE SESSION
		v_repExtPQBoxMsgCommunication("No connection with client.");

		v_repExtPQBarMsgCommunication("Disconnected from client.");
	}
}

/****************************************************************************************
										SERVER
****************************************************************************************/

Server::Server(boost::asio::io_service* io_service, short port1, short port2) : io_service_(io_service),
	acceptor_echo_(*io_service, tcp::endpoint(tcp::v4(), port1)),
	acceptor_send_(*io_service, tcp::endpoint(tcp::v4(), port2))
{
	start_accept();
}

void Server::start_accept()
{
	session_echo = std::make_shared<SessionEcho>(io_service_);
	session_send = std::make_shared<SessionSend>(io_service_);

	acceptor_echo_.async_accept(session_echo->get_socket(),
		boost::bind(&Server::handle_accept_echo, this,
			boost::asio::placeholders::error));

	acceptor_send_.async_accept(session_send->get_socket(),
		boost::bind(&Server::handle_accept_send, this,
			boost::asio::placeholders::error));
}

void Server::handle_accept_echo(const boost::system::error_code& error)
{
	if (!error)
	{
		session_echo->start();

		//Send message to simulator
		v_repExtPQBarMsgCommunication("Connected to client...");
	}
	else
	{
		//SIGNAL MANAGER TO DELETE SERVER
	}
}

void Server::handle_accept_send(const boost::system::error_code& error)
{
	if (!error)
	{
	}
	else
	{
		//SIGNAL MANAGER TO DELETE SERVER
	}
}

void Server::broadcast(int line, bool collision)
{
	session_send->deliver(line, collision);
}

void Server::cancel_sessions()
{
	try {
		boost::system::error_code ec_echo;
		session_echo->get_socket().shutdown(boost::asio::ip::tcp::socket::shutdown_both, ec_echo);
		session_echo->get_socket().close();
		
		boost::system::error_code ec_send;
		session_send->get_socket().shutdown(boost::asio::ip::tcp::socket::shutdown_both, ec_send);
		session_send->get_socket().close();
		
		acceptor_echo_.cancel();
		acceptor_send_.cancel();
	}
	catch (std::exception &ex)
	{
		v_repExtPQBoxMsgCommunication(ex.what());
	}
}

CommunicationManager::CommunicationManager()
{
}

CommunicationManager::~CommunicationManager()
{
}

std::shared_ptr<Server> CommunicationManager::get_server()
{
	return server;
}

void CommunicationManager::LaunchServer()
{
	try
	{
		int port1 = 1313;
		int port2 = 1315;

		server = std::make_shared<Server>(&ios, port1, port2);

		//launch io_service in another thread
		bt = std::make_unique<boost::thread>(boost::bind(&boost::asio::io_service::run, &ios));
	}
	catch (std::exception& e)
	{
		//Send message to simulator
		v_repExtPQBarMsgCommunication(e.what());
	}
}

void CommunicationManager::StopServer()
{
	try 
	{
		server->cancel_sessions();

		//ios.stop();
		bt->join();
	}
	catch (std::exception& e)
	{
		//Send message to simulator
		v_repExtPQBarMsgCommunication(e.what());
	}
}


