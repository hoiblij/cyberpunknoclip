using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Ownskit.Utils;

namespace Cyberpunk_trainer
{
    public partial class Form1 : Form
    {
        private Process gameProc = Process.GetProcessesByName("Cyberpunk2077").FirstOrDefault();
        private KeyboardListener keyboardListener = new KeyboardListener(); // not made by me
        private IntPtr hProc;
        private ProcessModule mainMod;
        private ProcessModule physMod;

        private IntPtr xAddress;
        private IntPtr yAddress;
        private IntPtr zAddress;

        private IntPtr xSpeedAddress;
        private IntPtr ySpeedAddress;
        private IntPtr zSpeedAddress;
        private IntPtr rotAddress;
        private IntPtr orientAddress;

        private bool glide = false;
        private int forward= 0;
        private int side = 0;
        private int up = 0;
        private int shift = 1;

        public Form1()
        {
            InitializeComponent();
            hProc = gameProc.Handle;
            mainMod = gameProc.MainModule;
            foreach (ProcessModule mod in gameProc.Modules)
            {
                if (mod.ModuleName == "PhysX3_x64.dll") // positions and speeds of player are in the physx3 dll
                {
                    physMod = mod;
                    break;
                }
            }

            timer2.Start();
            timer.Start();

            keyboardListener.KeyDown += KListener_KeyDown;
            keyboardListener.KeyUp += KListener_KeyUp;
        }

        private void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if (args.Key == System.Windows.Input.Key.Y)
                up = 2;
            if (args.Key == System.Windows.Input.Key.H)
                up = -1;
            if (args.Key == System.Windows.Input.Key.U)
                glide = !glide;

            if (args.Key == System.Windows.Input.Key.A)
                side = -1;
            if (args.Key == System.Windows.Input.Key.D)
                side = 1;
            if (args.Key == System.Windows.Input.Key.S)
                forward = 1;
            if (args.Key == System.Windows.Input.Key.W)
                forward = -1;
            if (args.Key == System.Windows.Input.Key.LeftShift)
                shift = 2;
        }

        private void KListener_KeyUp(object sender, RawKeyEventArgs args)
        {
           if (args.Key == System.Windows.Input.Key.Y)
               up = 0;
           if (args.Key == System.Windows.Input.Key.H)
               up = 0;

            if (args.Key == System.Windows.Input.Key.A)
                side = 0;
            if (args.Key == System.Windows.Input.Key.D)
                side = 0;
            if (args.Key == System.Windows.Input.Key.S)
                forward = 0;
            if (args.Key == System.Windows.Input.Key.W)
                forward = 0;
            if (args.Key == System.Windows.Input.Key.LeftShift)
                shift = 1;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (glide)
            {
                MemoryFunctions.WriteMem(hProc, xSpeedAddress, 0f);
                MemoryFunctions.WriteMem(hProc, ySpeedAddress, 0f);
                MemoryFunctions.WriteMem(hProc, zSpeedAddress, 0f);
            }

            if (up != 0)
            {
                double currentY = MemoryFunctions.ReadMemDouble(hProc, yAddress);
                MemoryFunctions.WriteMem(hProc, yAddress, currentY + up * 1);
            }

            float angle = MemoryFunctions.ReadMem(hProc, rotAddress);
            float orient = MemoryFunctions.ReadMem(hProc, orientAddress);
            //looks confusing, its really just figuring shit out by moving the mouse and looking at the values
            //angle goes in the 0-90 range from 0-1, 90-180 from 1-0, 180-270 from 0-(-1), 270-360 from (-1)-0
            //used another value I called orientation to figure out how the angle should be mapped to go to 0-360
            if (angle >= 0 && orient >= 0) 
                angle = angle * 90;
            else if (angle >= 0 && orient < 0)
                angle = (1 - angle) * 90 + 90;
            else if (angle < 0 && orient <= 0)
                angle = angle * -90 + 180;
            else if (angle < 0 && orient > 0)
                angle = (angle + 1) * 90 + 270;

            angle = (float)(angle / 180 * Math.PI); //to radians

            if (glide)
            {
                double currentX = MemoryFunctions.ReadMemDouble(hProc, xAddress);
                double currentZ = MemoryFunctions.ReadMemDouble(hProc, zAddress);
                double x = 0, z = 0;

                if (forward != 0)
                {
                    x = Math.Sin(angle + Math.PI) * forward;
                    z = Math.Cos(angle + Math.PI) * forward;
                }

                if (side != 0)
                {
                    x += Math.Sin(angle + Math.PI / 2) * side;
                    z += Math.Cos(angle + Math.PI / 2) * side;
                }
                MemoryFunctions.WriteMem(hProc, xAddress, currentX + (double)speed.Value * x * shift);
                MemoryFunctions.WriteMem(hProc, zAddress, currentZ + (double)speed.Value * z * shift);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //addresses tend to change when the player dies, so we update them every 5 seconds
            xAddress = MemoryFunctions.GetDMAAddy(hProc, physMod.BaseAddress + 0x25FC20, new IntPtr[] { (IntPtr)0x58, (IntPtr)0x0, (IntPtr)0x60, (IntPtr)0x0, (IntPtr)0x50, (IntPtr)0x20, (IntPtr)0x210 });
            yAddress = xAddress + 0x10;
            zAddress = xAddress + 0x8;
            xSpeedAddress = MemoryFunctions.GetDMAAddy(hProc, physMod.BaseAddress + 0x25FC20, new IntPtr[] { (IntPtr)0x58, (IntPtr)0x0, (IntPtr)0x8, (IntPtr)0x8, (IntPtr)0xd8, (IntPtr)0x0, (IntPtr)0x114 });
            ySpeedAddress = xSpeedAddress + 0x8;
            zSpeedAddress = xSpeedAddress + 0x4;
            orientAddress = MemoryFunctions.GetDMAAddy(hProc, mainMod.BaseAddress + 0x463C2E0, new IntPtr[] { (IntPtr)0x28, (IntPtr)0x18, (IntPtr)0x68, (IntPtr)0x20, (IntPtr)0xb0, (IntPtr)0xf8, (IntPtr)0x0 });
            rotAddress = orientAddress + 0x10;
        }
    }
}
