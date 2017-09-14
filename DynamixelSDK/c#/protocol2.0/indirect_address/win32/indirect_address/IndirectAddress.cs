/*******************************************************************************
* Copyright (c) 2016, ROBOTIS CO., LTD.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
* * Redistributions of source code must retain the above copyright notice, this
*   list of conditions and the following disclaimer.
*
* * Redistributions in binary form must reproduce the above copyright notice,
*   this list of conditions and the following disclaimer in the documentation
*   and/or other materials provided with the distribution.
*
* * Neither the name of ROBOTIS nor the names of its
*   contributors may be used to endorse or promote products derived from
*   this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
* FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
* DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
* SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
* CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
* OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
* OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*******************************************************************************/

/* Author: Ryu Woon Jung (Leon) */

//
// *********     Indirect Address Example      *********
//
//
// Available Dynamixel model on this example : All models using Protocol 2.0
// This example is designed for using a Dynamixel PRO 54-200, and an USB2DYNAMIXEL.
// To use another Dynamixel model, such as X series, see their details in E-Manual(support.robotis.com) and edit below variables yourself.
// Be sure that Dynamixel PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using dynamixel_sdk;

namespace sync_write
{
  class SyncWrite
  {
    // Control table address                                                        // Control table address is different in Dynamixel model
    public const int ADDR_PRO_INDIRECTADDRESS_FOR_WRITE      = 49;                  // EEPROM region
    public const int ADDR_PRO_INDIRECTADDRESS_FOR_READ       = 59;                  // EEPROM region
    public const int ADDR_PRO_TORQUE_ENABLE                  = 562;
    public const int ADDR_PRO_LED_RED                        = 563;
    public const int ADDR_PRO_GOAL_POSITION                  = 596;
    public const int ADDR_PRO_MOVING                         = 610;
    public const int ADDR_PRO_PRESENT_POSITION               = 611;
    public const int ADDR_PRO_INDIRECTDATA_FOR_WRITE         = 634;
    public const int ADDR_PRO_INDIRECTDATA_FOR_READ          = 639;

    // Data Byte Length
    public const int LEN_PRO_LED_RED                         = 1;
    public const int LEN_PRO_GOAL_POSITION                   = 4;
    public const int LEN_PRO_MOVING                          = 1;
    public const int LEN_PRO_PRESENT_POSITION                = 4;
    public const int LEN_PRO_INDIRECTDATA_FOR_WRITE          = 5;
    public const int LEN_PRO_INDIRECTDATA_FOR_READ           = 5;

    // Protocol version
    public const int PROTOCOL_VERSION                        = 2;                   // See which protocol version is used in the Dynamixel

    // Default setting
    public const int DXL_ID                                  = 1;                   // Dynamixel ID: 1
    public const int BAUDRATE                                = 57600;
    public const string DEVICENAME                           = "COM1";              // Check which port is being used on your controller
                                                                                    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int TORQUE_ENABLE                           = 1;                   // Value for enabling the torque
    public const int TORQUE_DISABLE                          = 0;                   // Value for disabling the torque
    public const int DXL_MINIMUM_POSITION_VALUE              = -150000;             // Dynamixel will rotate between this value
    public const int DXL_MAXIMUM_POSITION_VALUE              = 150000;              // and this value (note that the Dynamixel would not move when the position value is out of movable range. Check e-manual about the range of the Dynamixel you use.)
    public const int DXL_MINIMUM_LED_VALUE                   = 0;                   // Dynamixel LED will light between this value
    public const int DXL_MAXIMUM_LED_VALUE                   = 255;                 // and this value
    public const int DXL_MOVING_STATUS_THRESHOLD             = 20;                  // Dynamixel moving status threshold

    public const byte ESC_ASCII_VALUE                        = 0x1b;

    public const int COMM_SUCCESS                            = 0;                   // Communication Success result value
    public const int COMM_TX_FAIL                            = -1001;               // Communication Tx Failed

    static void Main(string[] args)
    {
      // Initialize PortHandler Structs
      // Set the port path
      // Get methods and members of PortHandlerLinux or PortHandlerWindows
      int port_num = dynamixel.portHandler(DEVICENAME);

      // Initialize PacketHandler Structs
      dynamixel.packetHandler();

      // Initialize Groupsyncwrite instance
      int groupwrite_num = dynamixel.groupSyncWrite(port_num, PROTOCOL_VERSION, ADDR_PRO_INDIRECTDATA_FOR_WRITE, LEN_PRO_INDIRECTDATA_FOR_WRITE);

      // Initialize Groupsyncread Structs for Present Position
      int groupread_num = dynamixel.groupSyncRead(port_num, PROTOCOL_VERSION, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_INDIRECTDATA_FOR_READ);

      int index = 0;
      int dxl_comm_result = COMM_TX_FAIL;                     // Communication result
      bool dxl_addparam_result = false;                       // AddParam result
      bool dxl_getdata_result = false;                        // GetParam result
      int[] dxl_goal_position = new int[2]{ DXL_MINIMUM_POSITION_VALUE, DXL_MAXIMUM_POSITION_VALUE };         // Goal position

      byte dxl_error = 0;                                     // Dynamixel error
      byte dxl_moving = 0;                                    // Dynamixel moving status
      byte[] dxl_led_value = new byte[2]{ 0x00, 0xFF };       // Dynamixel LED value
      Int32 dxl_present_position = 0;                         // Present position

      // Open port
      if (dynamixel.openPort(port_num))
      {
        Console.WriteLine("Succeeded to open the port!");
      }
      else
      {
        Console.WriteLine("Failed to open the port!");
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
        return;
      }

      // Set port baudrate
      if (dynamixel.setBaudRate(port_num, BAUDRATE))
      {
        Console.WriteLine("Succeeded to change the baudrate!");
      }
      else
      {
        Console.WriteLine("Failed to change the baudrate!");
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
        return;
      }

      // Disable Dynamixel Torque :
      // Indirect address would not accessible when the torque is already enabled
      dynamixel.write1ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }
      else
      {
        Console.WriteLine("Dynamixel has been successfully connected");
      }

      // INDIRECTDATA parameter storages replace LED, goal position, present position and moving status storages
      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 0, ADDR_PRO_GOAL_POSITION + 0);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 2, ADDR_PRO_GOAL_POSITION + 1);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 4, ADDR_PRO_GOAL_POSITION + 2);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 6, ADDR_PRO_GOAL_POSITION + 3);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 8, ADDR_PRO_LED_RED);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 0, ADDR_PRO_PRESENT_POSITION + 0);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 2, ADDR_PRO_PRESENT_POSITION + 1);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 4, ADDR_PRO_PRESENT_POSITION + 2);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 6, ADDR_PRO_PRESENT_POSITION + 3);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      dynamixel.write2ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 8, ADDR_PRO_MOVING);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      // Enable Dynamixel Torque
      dynamixel.write1ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      // Add parameter storage for Dynamixel present position value
      dxl_addparam_result = dynamixel.groupSyncReadAddParam(groupread_num, DXL_ID);
      if (dxl_addparam_result != true)
      {
        Console.WriteLine("[ID: {0}] groupSyncRead addparam failed", DXL_ID);
        return;
      }

      while (true)
      {
        Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
        if (Console.ReadKey().KeyChar == ESC_ASCII_VALUE)
          break;

        // Add values to the Syncwrite storage
        dxl_addparam_result = dynamixel.groupSyncWriteAddParam(groupwrite_num, DXL_ID, (UInt32)dxl_goal_position[index], LEN_PRO_GOAL_POSITION);
        if (dxl_addparam_result != true)
        {
          Console.WriteLine("[ID: {0}] groupSyncWrite addparam failed", DXL_ID);
          return;
        }
        dxl_addparam_result = dynamixel.groupSyncWriteAddParam(groupwrite_num, DXL_ID, dxl_led_value[index], LEN_PRO_LED_RED);
        if (dxl_addparam_result != true)
        {
          Console.WriteLine("[ID: {0}] groupSyncWrite addparam failed", DXL_ID);
          return;
        }

        // Syncwrite goal position
        dynamixel.groupSyncWriteTxPacket(groupwrite_num);
        if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
          Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));

        // Clear syncwrite parameter storage
        dynamixel.groupSyncWriteClearParam(groupwrite_num);

        do
        {
          // Syncread present position from indirectdata2
          dynamixel.groupSyncReadTxRxPacket(groupread_num);
          if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));

          // Check if groupsyncread data of Dyanamixel is available
          dxl_getdata_result = dynamixel.groupSyncReadIsAvailable(groupread_num, DXL_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION);
          if (dxl_getdata_result != true)
          {
            Console.WriteLine("[ID: {0}] groupSyncRead getdata failed", DXL_ID);
            return;
          }

          // Check if groupsyncread data of Dyanamixel is available
          dxl_getdata_result = dynamixel.groupSyncReadIsAvailable(groupread_num, DXL_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING);
          if (dxl_getdata_result != true)
          {
            Console.WriteLine("[ID: {0}] groupSyncRead getdata failed", DXL_ID);
            return;
          }

          // Get Dynamixel present position value
          dxl_present_position = (Int32)dynamixel.groupSyncReadGetData(groupread_num, DXL_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION);

          // Get Dynamixel moving status value
          dxl_moving = (byte)dynamixel.groupSyncReadGetData(groupread_num, DXL_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING);

          Console.WriteLine("[ID: {0}] GoalPos: {1}  PresPos: {2}  IsMoving: {3}", DXL_ID, dxl_goal_position[index], dxl_present_position, dxl_moving);

        } while (Math.Abs(dxl_goal_position[index] - dxl_present_position) > DXL_MOVING_STATUS_THRESHOLD);

        // Change goal position
        if (index == 0)
        {
          index = 1;
        }
        else
        {
          index = 0;
        }
      }

      // Disable Dynamixel Torque
      dynamixel.write1ByteTxRx(port_num, PROTOCOL_VERSION, DXL_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((dxl_comm_result = dynamixel.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
      }
      else if ((dxl_error = dynamixel.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
      }

      // Close port
      dynamixel.closePort(port_num);

      return;
    }
  }
}
