#include <iostream>
#include <boost/asio.hpp>
#include <boost/asio/ip/address.hpp>

using namespace std;
using namespace boost::asio;
using namespace boost;

int main() {
	io_service ios;
	cout << "server start." << endl;
	ip::tcp::acceptor acc(ios, ip::tcp::endpoint(ip::tcp::v4(), 6688));
	cout << acc.local_endpoint().address() << endl;
	while (true) {
		ip::tcp::socket sock(ios);
		acc.accept(sock);
		cout << sock.remote_endpoint().address() << endl;
		sock.write_some(buffer("hello asio"));
	}
	return 0;
}