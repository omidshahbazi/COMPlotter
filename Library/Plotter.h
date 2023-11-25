#pragma once
#ifndef PLOTTER__H
#define PLOTTER__H

#include <stdio.h>
#include <chrono>
#include <mutex>
#include <sstream>
#include <driver/uart.h>

using namespace std;
using namespace std::chrono;

class Plotter
{
private:
	class PointInfo
	{
	public:
		uint8 ID;
		// float Time;
		int32 Color;
		double Value;
	};

public:
	Plotter(void)
		: m_PointCount(0),
		  m_Beginning(system_clock::now())
	{
		Serial.begin(115200);

		// right after starting UART0, add this code:
		uart_intr_config_t uart_intr = {
			.intr_enable_mask = (0x1 << 0) | (0x8 << 0), // UART_INTR_RXFIFO_FULL | UART_INTR_RXFIFO_TOUT,
			.rx_timeout_thresh = 1,
			.txfifo_empty_intr_thresh = 10,
			.rxfifo_full_thresh = 112,
		};
		uart_intr_config((uart_port_t)0, &uart_intr); // Zero is the UART number for Arduino Serial
	}

	template <typename A>
	void Plot(uint8 ID, A Value)
	{
		Plot(ID, 0, 0, 255, Value);
	}

	template <typename T>
	void Plot(uint8 ID, uint8 Red, uint8 Green, uint8 Blue, T Value)
	{
		m_Lock.lock();

		PointInfo info;
		info.ID = ID;
		// info.Time = duration_cast<milliseconds>(system_clock::now() - m_Beginning).count() / 1000.0F;
		info.Color = (Red << 16) | (Green << 8) | Blue;
		info.Value = Value;

		m_Points[m_PointCount++] = info;

		m_Lock.unlock();

		if (m_PointCount == BUFFER_LENGTH)
		{
			Flush();
			m_PointCount = 0;
		}
	}

	void Flush(void)
	{
		m_Lock.lock();

		stringstream ss;

		ss << "[";

		for (uint8 i = 0; i < m_PointCount; ++i)
		{
			const PointInfo &info = m_Points[i];

			if (i != 0)
				ss << ",";

			//\"T\":" << info.Time << ",
			ss << "{\"I\":" << (uint32)info.ID << ",\"C\":" << info.Color << ",\"V\":" << info.Value << "}";
		}

		ss << "]\n";

		// printf(ss.str().c_str());
		Serial.write(ss.str().c_str());

		m_PointCount = 0;

		m_Lock.unlock();
	}

private:
	static const uint8 BUFFER_LENGTH = 64;

	system_clock::time_point m_Beginning;

	PointInfo m_Points[BUFFER_LENGTH];
	uint8 m_PointCount;

	mutex m_Lock;
};

#endif