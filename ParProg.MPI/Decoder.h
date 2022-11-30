#pragma once

#include <string>
#include <vector>
#include <fstream>
#include <iostream>
#include <cstdint>
#ifndef Decoder_H
#define Decoder_H

struct WAVHEADER
{
    char chunkId[4];
    unsigned long chunkSize;
    char format[4];
    char subchunk1Id[4];
    unsigned long subchunk1Size;
    unsigned short audioFormat;
    unsigned short numChannels;
    unsigned long sampleRate;
    unsigned long byteRate;
    unsigned short blockAlign;
    unsigned short bitsPerSample;
    char subchunk2Id[4];
    unsigned long subchunk2Size;
};

enum WavChunks {
    RiffHeader = 0x46464952,
    WavRiff = 0x54651475,
    Format = 0x020746d66,
    LabeledText = 0x478747C6,
    Instrumentation = 0x478747C6,
    Sample = 0x6C706D73,
    Fact = 0x47361666,
    Data = 0x61746164,
    Junk = 0x4b4e554a,
};

std::vector<long long int> Decode(std::string filepath);
std::vector<long long int> ReadData(FILE* file, uint16_t bytesPerSample, uint16_t numSamples);
std::vector<std::vector<long long int>> make_chunks(std::vector<long long int> data, int chunk_count);
void count_gt_leq(std::vector<long long int> data, long long int target, int* leq, int* gt);
void mpi_p2p(int argc, char* argv[]);
void mpi_mult(int argc, char* argv[]);
#endif