#include "server.h"

int main()
{
	Server server = Server();
	server.Initialize(PORT);
	server.Start();

	return 0;
}