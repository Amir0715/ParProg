#include <iostream>
#include "Decoder.h"
#include <time.h>

int main(int argc, char* argv[])
{
    // MPI точка - точка
    // mpi_p2p(argc, argv);

    // MPI мултплексивный обмен
    mpi_mult(argc, argv);
}