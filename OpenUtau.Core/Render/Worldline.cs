﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using NAudio.Wave;
using OpenUtau.Classic;
using OpenUtau.Core.Format;
using Serilog;

namespace OpenUtau.Core.Render {
    static class Worldline {
        [DllImport("worldline", CallingConvention = CallingConvention.Cdecl)]
        static extern int DecodeMgc(
            int f0Length, double[] mgc, int mgcSize,
            int fftSize, int fs, ref IntPtr spectrogram);

        public static double[,] DecodeMgc(int f0Length, double[] mgc, int fftSize, int fs) {
            try {
                int mgcSize = mgc.Length / f0Length;
                unsafe {
                    IntPtr buffer = IntPtr.Zero;
                    int size = DecodeMgc(f0Length, mgc, mgcSize, fftSize, fs, ref buffer);
                    var data = new double[f0Length * size];
                    Marshal.Copy(buffer, data, 0, data.Length);
                    Marshal.FreeCoTaskMem(buffer);
                    var output = new double[f0Length, size];
                    Buffer.BlockCopy(data, 0, output, 0, data.Length * sizeof(double));
                    return output;
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to decode.");
                return null;
            }
        }

        [DllImport("worldline", CallingConvention = CallingConvention.Cdecl)]
        static extern int DecodeBap(
            int f0Length, double[] bap,
            int fftSize, int fs, ref IntPtr aperiodicity);

        public static double[,] DecodeBap(int f0Length, double[] bap, int fftSize, int fs) {
            try {
                unsafe {
                    IntPtr buffer = IntPtr.Zero;
                    int size = DecodeBap(f0Length, bap, fftSize, fs, ref buffer);
                    var data = new double[f0Length * size];
                    Marshal.Copy(buffer, data, 0, data.Length);
                    Marshal.FreeCoTaskMem(buffer);
                    var output = new double[f0Length, size];
                    Buffer.BlockCopy(data, 0, output, 0, data.Length * sizeof(double));
                    return output;
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to decode.");
                return null;
            }
        }

        [DllImport("worldline", CallingConvention = CallingConvention.Cdecl)]
        static extern int WorldSynthesis(
            double[] f0, int f0Length,
            double[,] mgcOrSp, bool isMgc, int mgcSize,
            double[,] bapOrAp, bool isBap, int fftSize,
            double framePeriod, int fs, ref IntPtr y,
            double[] gender, double[] tension,
            double[] breathiness, double[] voicing);

        public static double[] WorldSynthesis(
            double[] f0,
            double[,] mgcOrSp, bool isMgc, int mgcSize,
            double[,] bapOrAp, bool isBap, int fftSize,
            double framePeriod, int fs,
            double[] gender, double[] tension,
            double[] breathiness, double[] voicing) {
            unsafe {
                IntPtr buffer = IntPtr.Zero;
                int size = WorldSynthesis(
                    f0, f0.Length,
                    mgcOrSp, isMgc, mgcSize,
                    bapOrAp, isBap, fftSize,
                    framePeriod, fs, ref buffer,
                    gender, tension, breathiness, voicing);
                var data = new double[size];
                Marshal.Copy(buffer, data, 0, size);
                Marshal.FreeCoTaskMem(buffer);
                return data;
            }
        }

        [DllImport("worldline", CallingConvention = CallingConvention.Cdecl)]
        static extern int WorldSynthesis(
            double[] f0, int f0Length,
            double[] mgcOrSp, bool isMgc, int mgcSize,
            double[] bapOrAp, bool isBap, int fftSize,
            double framePeriod, int fs, ref IntPtr y,
            double[] gender, double[] tension,
            double[] breathiness, double[] voicing);

        public static double[] WorldSynthesis(
            double[] f0,
            double[] mgcOrSp, bool isMgc, int mgcSize,
            double[] bapOrAp, bool isBap, int fftSize,
            double framePeriod, int fs,
            double[] gender, double[] tension,
            double[] breathiness, double[] voicing) {
            unsafe {
                IntPtr buffer = IntPtr.Zero;
                int size = WorldSynthesis(
                    f0, f0.Length,
                    mgcOrSp, isMgc, mgcSize,
                    bapOrAp, isBap, fftSize,
                    framePeriod, fs, ref buffer,
                    gender, tension, breathiness, voicing);
                var data = new double[size];
                Marshal.Copy(buffer, data, 0, size);
                Marshal.FreeCoTaskMem(buffer);
                return data;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SynthRequest {
            public int sample_fs;
            public int sample_length;
            public IntPtr sample;
            public int tone;
            public double con_vel;
            public double offset;
            public double required_length;
            public double consonant;
            public double cut_off;
            public double volume;
            public double modulation;
            public double tempo;
            public int pitch_bend_length;
            public IntPtr pitch_bend;
            public int flag_g;
            public int flag_O;
            public int flag_P;
            public int flag_Mt;
            public int flag_Mb;
            public int flag_Mv;
        };

        [DllImport("worldline")]
        static extern int Resample(IntPtr request, ref IntPtr y);

        public static float[] Resample(ResamplerItem item) {
            int fs;
            double[] sample;
            using (var waveStream = Wave.OpenFile(item.inputTemp)) {
                fs = waveStream.WaveFormat.SampleRate;
                sample = Wave.GetSamples(waveStream.ToSampleProvider().ToMono(1, 0))
                    .Select(f => (double)f).ToArray();
            }

            var pinnedSample = GCHandle.Alloc(sample, GCHandleType.Pinned);
            var pinnedPitchBend = GCHandle.Alloc(item.pitches, GCHandleType.Pinned);
            var request = new SynthRequest {
                sample_fs = fs,
                sample_length = sample.Length,
                sample = pinnedSample.AddrOfPinnedObject(),
                tone = item.tone,
                con_vel = item.velocity,
                offset = item.offset,
                required_length = item.requiredLength,
                consonant = item.consonant,
                cut_off = item.cutoff,
                volume = item.volume,
                modulation = item.modulation,
                tempo = item.tempo,
                pitch_bend_length = item.pitches.Length,
                pitch_bend = pinnedPitchBend.AddrOfPinnedObject(),
                flag_g = 0,
                flag_O = 0,
                flag_P = 86,
                flag_Mt = 0,
                flag_Mb = 0,
                flag_Mv = 100,
            };
            var flag = item.flags.FirstOrDefault(f => f.Item1 == "g");
            if (flag != null && flag.Item2.HasValue) {
                request.flag_g = flag.Item2.Value;
            }
            flag = item.flags.FirstOrDefault(f => f.Item1 == "O");
            if (flag != null && flag.Item2.HasValue) {
                request.flag_O = flag.Item2.Value;
            }
            flag = item.flags.FirstOrDefault(f => f.Item1 == "P");
            if (flag != null && flag.Item2.HasValue) {
                request.flag_P = flag.Item2.Value;
            }
            flag = item.flags.FirstOrDefault(f => f.Item1 == "Mt");
            if (flag != null && flag.Item2.HasValue) {
                request.flag_Mt = flag.Item2.Value;
            }
            flag = item.flags.FirstOrDefault(f => f.Item1 == "Mb");
            if (flag != null && flag.Item2.HasValue) {
                request.flag_Mb = flag.Item2.Value;
            }
            flag = item.flags.FirstOrDefault(f => f.Item1 == "Mv");
            if (flag != null && flag.Item2.HasValue) {
                request.flag_Mv = flag.Item2.Value;
            }

            try {
                unsafe {
                    IntPtr buffer = IntPtr.Zero;
                    int size = Resample(new IntPtr(&request), ref buffer);
                    var data = new float[size];
                    Marshal.Copy(buffer, data, 0, size);
                    Marshal.FreeCoTaskMem(buffer);
                    return data;
                }
            } finally {
                pinnedSample.Free();
                pinnedPitchBend.Free();
            }
        }
    }
}
