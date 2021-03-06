﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDL2;

using System.Runtime.InteropServices;

namespace BSPDTest
{
    class Program
    {

        [DllImport("BSPD")]
        private static extern void BSPDTest();



        static string yuvfilepath = "./test.yuv";
         // static string input = "rtmp://live.hkstv.hk.lxdns.com/live/hks";
        static BSPDMediaSource BSPDMS;


        static IntPtr mwindow;
        static IntPtr mrender;
        static IntPtr mtex;
        static int w, h;
        static SDL.SDL_Rect mrect;

        static void CreateRender()
        {
            SDL.SDL_Init(SDL.SDL_INIT_AUDIO | SDL.SDL_INIT_VIDEO);
            mwindow = SDL.SDL_CreateWindow("test", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, w, h, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
            mrender = SDL.SDL_CreateRenderer(mwindow, -1, 0);
        }
       static byte[] temp = null;

        static void SDLCALL(
             byte[] userdata,
            IntPtr stream,
            int len
            )
        {
            if (temp == null)
            {
                temp = new byte[1000];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = (byte)(255.0 * Math.Sin(i*3.14/1000));
                }
            }
            if (len>0)
            {
                SDL.SDL_MixAudio(stream, temp, (uint)len, SDL.SDL_MIX_MAXVOLUME);
            }
        }

        static SDL.SDL_Event ev;
        static SDL.SDL_AudioSpec wanted_spec = new SDL.SDL_AudioSpec();
        static void Loop()
        {
            while (SDL.SDL_PollEvent(out ev)>0)
            {
                switch (ev.type)
                {
                 
                    case SDL.SDL_EventType.SDL_QUIT:
                        break;
                    default:
                        break;
                }
            }

            if (w > 0 && IntPtr.Zero == mtex)
            {
                mtex = SDL.SDL_CreateTexture(mrender, SDL.SDL_PIXELFORMAT_IYUV, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, w, h);
                mrect.h = h;
                mrect.w = w;
                mrect.x = mrect.y = 0;
                int ysize = w * h;
                ydata = new byte[(int)(ysize)];
                udata = new byte[ysize/4 ];
                vdata = new byte[ysize/4 ];
                wanted_spec.channels = 2;
                wanted_spec.format = SDL.AUDIO_F32;
                wanted_spec.freq = 44100;
                wanted_spec.silence = 0;
                wanted_spec.samples = 1500;
                wanted_spec.callback = SDLCALL;
                SDL.SDL_AudioSpec out_spec = new SDL.SDL_AudioSpec();
                SDL.SDL_OpenAudio(ref wanted_spec, out out_spec);
                SDL.SDL_PauseAudio(0);
            }

            BSPDMS.GetRawData(ydata, udata, vdata);

            Update();

        }


        static byte[] ydata;
        static byte[] udata;
        static byte[] vdata;
        static void Update()
        {
            if (mtex == IntPtr.Zero)
            {
                return;
            }
            SDL.SDL_UpdateYUVTexture(mtex, ref mrect, ydata, w, udata, w/2, vdata, w/2);
            SDL.SDL_RenderClear(mrender);
            SDL.SDL_RenderCopy(mrender, mtex,IntPtr.Zero,ref mrect);
            SDL.SDL_RenderPresent(mrender);
        }
        
        static void Main(string[] args)
        {

            BSPDMS = new BSPDMediaSource();
            BSPDMS.OpenMedia("f:/sv.mp4");
            BSPDMS.GetDecWH(ref w, ref h);
            CreateRender();
            //   System.Threading.Thread.Sleep(10);

            int sw = 0;
            while (true)
            {
                Loop();

                //if (sw>=10)
                //{
                //    Thread.Sleep(70);
                //    sw = 0;
                //}
                Thread.Sleep(20);
                //sw++;

            }

        }

        private static void BSPDMS_MediaStateChage(BSPDMediaSource.BSPDMediaState sate)
        {
            Console.WriteLine("state:" + sate);
        }
    }
}
