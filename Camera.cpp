// Camera.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#pragma comment(lib, "ws2_32.lib")
#include <opencv2\highgui\highgui.hpp>
#include <opencv2\imgproc\imgproc.hpp>
#include <iostream>
#include <string>
#include "ImgProcess.h"
#include "WinCapture.h"
#include "Key.h"

using namespace std;
using namespace cv;

// Fluobeam image area. //Autoscaled Image
#define FLUOBEAM_WIN_NAME "ActiveMovie Window"
#define FLUOBEAM_WIN_CLASS "VideoRenderer"

#define FLUO 0
#define ULTRA 1

const int CLIENT_PORT = 2000;
SOCKET client;

/*
* Initialize client socket.
* Pop up error message and return false if an error occurs.
*/
bool init_socket() {
	cout << "Initialize socket... ";
	WSADATA wsaData;
	int iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != NO_ERROR) {
		MessageBox(0, "WSAStartup function failed", "Error", MB_OK);
		return false;
	}

	client = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (client == INVALID_SOCKET) {
		MessageBox(0, "socket function failed", "Error", MB_OK);
		WSACleanup();
		return false;
	}

	cout << "done." << endl;
	return true;
}

/*
* Connect to server.
* Pop up error message and return false if an error occurs.
*/
bool connect_server(const char *ip) {
	cout << "Connect to server... ";

	sockaddr_in clientService;
	clientService.sin_family = AF_INET;
	clientService.sin_addr.s_addr = inet_addr(ip);
	clientService.sin_port = htons(CLIENT_PORT);//

	// Connect to server.
	int iResult = connect(client, (SOCKADDR *)&clientService, sizeof(clientService));
	if (iResult == SOCKET_ERROR) {
		MessageBox(0, "connect function failed", "Error", MB_OK);
		iResult = closesocket(client);
		if (iResult == SOCKET_ERROR)
			MessageBox(0, "closesocket function failed", "Error", MB_OK);
		WSACleanup();
		return false;
	}

	cout << "done." << endl;
	return true;
}

/*
* Close client socket.
*/
bool close_socket() {
	cout << "Close socket...";
	int iResult = closesocket(client);
	if (iResult == SOCKET_ERROR) {
		WSACleanup();
		return false;
	}

	WSACleanup();
	cout << "done." << endl;
	return true;
}

/*
* Send image data to server.
* Send the length of data first, then send image data.
* Return false if not successful.
*/
bool send_img(const unsigned char* img_data, int length) {
	// Converts a u_long from host to TCP/IP network byte order (which is big-endian).
	u_long nlength = htonl(length);
	if (send(client, (const char*)&nlength, 4, 0) != 4)
	{
		return false;
	}
	
	//int flag = FLUO;
	//u_long nflag = htonl(flag);
	//if (send(client, (const char*)&nflag, 4, 0) != 4)
	//{
	//	return false;
	//}
	//
	int sent = 0;
	while (length > 0) {
		int result = send(client, reinterpret_cast<const char*>(img_data + sent), length, 0);
		if (result == SOCKET_ERROR)
			return false;
		length -= result;
		sent += result;
	}
	return true;
}

int main()
{
	cout << "Press Enter to start...";
	getchar();

	if (!init_socket())
	{
		return -1;
	}

	 /*Parse IP from config.ini*/
	char *ip = new char[20]();
	GetPrivateProfileString("Config", "IP", NULL, ip, 20, ".\\config.ini");
	if (!connect_server(ip))
		return -1;
	delete[] ip;

	/*Parse F-Window name from config.ini*/
	int width, height;
	char *fwinname = new char[20]();
	GetPrivateProfileString("Config", "FWinName", NULL, fwinname, 20, ".\\config.ini");
	win_name = "11.flv";

	set_window(width, height);

	// Wait key to translate or zoom image.
	/*CreateThread(0, 0, LPTHREAD_START_ROUTINE(wait_key), 0, 0, 0);*/
	
	int i = 1;
	while (true) {
		Mat img;

		capture(img, 1);//��ȡͼ��

		vector<Mat> channels; //vector<Mat>�� ��������Ϊ���Mat���͵����������飩   
		split(img, channels);  //��ԭͼ�����ͨ�����룬����һ��3ͨ��ͼ��ת����Ϊ3����ͨ��ͼ��channels[0],channels[1] ,channels[2]  
		vector<Mat> mbgr(3);  //��������ΪMat�����鳤��Ϊ3�ı���mbgr  
		Mat hideChannel(img.size(), CV_8UC1, Scalar(0));//��Ҫ���ص�ͨ�����ߴ���srcImage��ͬ����ͨ����ɫͼ��  

														//ע�⣺0ͨ��ΪB������1ͨ��ΪG������2ͨ��ΪR��������Ϊ��RGBɫ�ʿռ���opencv��Ĭ��ͨ��˳��ΪBGR������  
														//��1����ʾ��ɫ��B-��ɫ������   


														//��2����ʾ��ɫ��G����  
		Mat imageG(img.size(), CV_8UC3);//�����ߴ���srcImage��ͬ����ͨ��ͼ��imageG  
		mbgr[0] = hideChannel;
		mbgr[1] = channels[1];
		mbgr[2] = hideChannel;
		merge(mbgr, imageG);//imageG Ϊ��ɫͨ��ͼ��

		//Rect rect(6,76, 702,403);//��ͼ���������ڻ�ô����Ӿ�������޷�ʹ�ã�����������档
		//Mat image_roi = img(rect);
		//imwrite("cut.jpg", image_roi);
		unsigned char *img_data;
		int length = img_encode(img, img_data);//ͼ���С
		
		if (!send_img(img_data, length))//����ͼ��
		{
			break;
		}	

	
		cout << "Sent" << length << endl;
		if (!close_socket())
			return -1;

		if (!init_socket())
		{
			return -1;
		}
		char *ip = new char[20]();
		GetPrivateProfileString("Config", "IP", NULL, ip, 20, ".\\config.ini");
		if (!connect_server(ip))
			return -1;

		delete[] ip;
		
		i++;
		if(i>10000)
		{
			break;
		}
	}

	/*destroyAllWindows();*/
	if (!close_socket())
		return -1;

	getchar();
    return 0;
}

