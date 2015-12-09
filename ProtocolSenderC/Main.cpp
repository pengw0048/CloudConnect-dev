#include <iostream>
#include <boost/asio.hpp>
#include <boost/asio/ip/address.hpp>

using namespace std;
using namespace boost::asio;
using namespace boost;

int main(int argc, char *argv[]) {
	if (argc != 2) return -1;
	io_service ios;
	cout << "client start." << endl;
	ip::tcp::socket sock(ios);
	ip::tcp::endpoint ep(ip::address::from_string(argv[1]), 6688);
	sock.connect(ep);
	vector<char> str(100, 0);
	sock.read_some(buffer(str));
	cout << "recive from" << sock.remote_endpoint().address();
	cout << &str[0] << endl;
	return 0;
}