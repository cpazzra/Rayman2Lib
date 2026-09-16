using System.Text;
namespace Rayman2Lib
{
    public class BNMFile
    {
        public class SoundFile
        {
            public string name;
            public int sampleRate;
            public int length;
            public byte[] data;

            public int fileListOffset;
            public int lengthFieldOffset;

            public void Save(Stream stream)
            {
                // Oftens shows as 22.1khz in OS descriptions 
                uint numsamples = 22050;
                // Mono audio
                ushort numchannels = 1;
                ushort samplelength = 1;

                BinaryWriter wr = new BinaryWriter(stream);

                wr.Write(Encoding.ASCII.GetBytes("RIFF"));
                wr.Write(36 + data.Length);
                wr.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                // 16-bit pcm formatting (default for audacity)
                wr.Write(16);
                wr.Write((ushort)1);

                wr.Write(numchannels);
                wr.Write(sampleRate);
                wr.Write(sampleRate * samplelength * numchannels);
                wr.Write((ushort)(samplelength * numchannels));
                wr.Write((ushort)(16));
                wr.Write(Encoding.ASCII.GetBytes("data"));
                wr.Write(data.Length);
                wr.Write(data);
                wr.Close();
            }
        }

        readonly byte[] header;
        readonly byte[] fileList;
        public List<SoundFile> soundFiles = [];

        public BNMFile(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var r = new BinaryReader(stream);

            header = r.ReadBytes(44);

            int eventCount = BitConverter.ToInt32(header, 8);
            int fileCount = BitConverter.ToInt32(header, 16);
            int fileListEndOffset = BitConverter.ToInt32(header, 20);
            int audioDataOffset = BitConverter.ToInt32(header, 24);

            int dataPos = fileListEndOffset;
            if (audioDataOffset - fileListEndOffset > 0)
            {
                stream.Seek(audioDataOffset, SeekOrigin.Begin);
            } else
            {
                stream.Seek(dataPos, SeekOrigin.Begin);
            }

            fileList = r.ReadBytes(fileListEndOffset - 44);
            using var mr = new BinaryReader(new MemoryStream(fileList));

            if (eventCount > 0)
            {
                for (int i = 0; i < eventCount; i++) mr.ReadBytes(32);
            }

            if (fileCount > 0)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    int currentEntryOffset = (int)mr.BaseStream.Position;

                    var id = mr.ReadByte();
                    mr.ReadBytes(3); // someValue 1-3
                    int type = mr.ReadInt32();
                    mr.ReadBytes(4); // someValue 4-7

                    int lengthOffset = (int)mr.BaseStream.Position;
                    int length = mr.ReadInt32();

                    if (type == 0xA)
                    {
                        length = mr.ReadInt32();
                        mr.ReadBytes(40);
                    } else
                    {
                        mr.ReadBytes(44);
                    }

                    var sampleRate = mr.ReadInt32();
                    mr.ReadBytes(8);
                    var nameBytes = mr.ReadBytes(20);
                    var name = Encoding.ASCII.GetString(nameBytes).Split('\0')[0];

                    if (name.Length == 0) name = "UNKNOWN_TYPE";
                    if (name.Contains(".apm")) length = 0;

                    if (type == 1 && !soundFiles.Any(f => f.name == name))
                    {
                        soundFiles.Add(new SoundFile {
                            name = name,
                            sampleRate = sampleRate,
                            length = length,
                            fileListOffset = currentEntryOffset,
                            lengthFieldOffset = lengthOffset,
                            data = r.ReadBytes(length)
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Rebuilds the BNM file. 
        /// replacementFiles: Dictionary where key is sound name and value is path to replacement .wav
        /// TODO: Odds are, does not account for .apm files. Likely requires extra parsing to get working correctly.
        /// </summary>
        public byte[] SaveBNM(Dictionary<string, string?> replacementFiles = null)
        {
            var updatedFileList = (byte[])fileList.Clone();
            var audioDataStream = new MemoryStream();

            foreach (var sound in soundFiles)
            {
                byte[] finalData = sound.data;

                if (replacementFiles != null &&
                    replacementFiles.TryGetValue(sound.name, out string? wavPath))
                {
                    finalData = ExtractRawPCMDataFromWAV(wavPath);
                    Console.WriteLine($"Replacement: {wavPath}, extracted {finalData.Length} bytes");
                }

                Console.WriteLine($"BNM entry: {sound.name}, original {sound.data.Length} bytes, final {finalData.Length} bytes");

                byte[] lenBytes = BitConverter.GetBytes(finalData.Length);
                Buffer.BlockCopy(lenBytes, 0, updatedFileList, sound.lengthFieldOffset, 4);
                audioDataStream.Write(finalData, 0, finalData.Length);
            }

            // Update Header size fields
            byte[] updatedHeader = (byte[])header.Clone();

            int fileListEnd = 44 + updatedFileList.Length;
            int audioLength = checked((int)audioDataStream.Length);
            int totalSize = fileListEnd + (int)audioDataStream.Length;

            // Header offset 20: end of file-list section
            Buffer.BlockCopy(
                BitConverter.GetBytes(fileListEnd),
                0,
                updatedHeader,
                20,
                4);

            // Header offset 24: audio-data start offset, according to your extractor
            Buffer.BlockCopy(
                BitConverter.GetBytes(fileListEnd),
                0,
                updatedHeader,
                24,
                4);

            using var finalStream = new MemoryStream();
            finalStream.Write(updatedHeader, 0, updatedHeader.Length);
            finalStream.Write(updatedFileList, 0, updatedFileList.Length);
            finalStream.Write(audioDataStream.ToArray(), 0, (int)audioDataStream.Length);

            Console.WriteLine($"File-list end: {fileListEnd}");
            Console.WriteLine($"Audio length: {audioLength}");
            Console.WriteLine($"Total size: {totalSize}");

            return finalStream.ToArray();
        }

        private static byte[] ExtractRawPCMDataFromWAV(string path)
        {
            using var fs = File.OpenRead(path);
            using var reader = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

            // Don't understand details of this header completely, but need to skip over when parsing WAV file.
            // Perform checks on wav header anyway.
            string riffId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (riffId != "RIFF")
            {
                throw new InvalidDataException("WAV missing root chunk.");
            }

            string waveId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (waveId != "WAVE")
            {
                throw new InvalidDataException("WAV missing RIFF type");
            }

            while (fs.Position + 8 <= fs.Length)
            {
                string chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
                uint chunkSize = reader.ReadUInt32();

                if (chunkSize > fs.Length - fs.Position)
                {
                    throw new InvalidDataException($"Invalid {chunkId} chunk size: {chunkSize}.");
                }

                // Actual PCM data chunk
                if (chunkId == "data")
                {
                    return reader.ReadBytes(checked((int)chunkSize));
                }
                // Skip a padding byte for odd sizes
                fs.Seek(chunkSize + (chunkSize & 1), SeekOrigin.Current);
            }

            throw new InvalidDataException("No data chunk found in WAV file.");
        }

    }
}
