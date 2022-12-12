#include "Decoder.h"


std::vector<long long int> ReadData(FILE* file, uint16_t bytesPerSample, uint16_t numSamples) {
	static const uint16_t BUFFER_SIZE = 4096;
	std::vector<long long int> data;
	size_t bytesRead;

	switch (bytesPerSample)
	{
	case 1:
		char int8;
		while (fread(&int8, sizeof(char), 1, file) > 0)
		{
			data.push_back((long long int)int8);
		}
		break;
	case 2:
		int int16;
		while (fread(&int16, sizeof(int), 1, file) > 0)
		{
			data.push_back((long long int)int16);
		}
		break;
	case 4:
		long int int32;
		while (fread(&int32, sizeof(long int), 1, file) > 0)
		{
			data.push_back((long long int)int32);
		}
		break;
	case 8:
		long long int int64;
		while (fread(&int64, sizeof(long long int), 1, file) > 0)
		{
			data.push_back(int64);
		}
		break;
	default:
		throw std::invalid_argument("PCM BitRate should be in (8, 16, 32, 64)!");
		break;
	}

	return data;
}

std::vector<long long int> Decode(std::string filepath) {
	std::vector<long long int> result;
	int headerSize = sizeof(WAVHEADER);
	WAVHEADER wavHeader;

	FILE* wavFile;
	fopen_s(&wavFile, filepath.c_str(), "rb+");
	if (wavFile == NULL)
	{
		printf("file not found");
		exit(-1);
	}

	size_t headerBytesRead = fread(&wavHeader, 1, headerSize, wavFile);
	if (headerBytesRead > 0) {
		uint16_t bytesPerSample = wavHeader.bitsPerSample / 8;
		uint16_t numSamples = wavHeader.chunkSize / bytesPerSample;

		result = ReadData(wavFile, bytesPerSample, numSamples);
	}

	fclose(wavFile);
	return result;
}

std::vector<std::vector<long long int>> make_chunks(std::vector<long long int> data, int chunk_count)
{
	std::vector<std::vector<long long int>> datas(chunk_count);

	for (int i = 0; i < data.size(); i++)
	{
		int chunk_index = i % chunk_count;
		datas[chunk_index].push_back(data[i]);
	}
	return datas;
}