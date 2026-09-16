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

            public ushort channels;
            public ushort bitsPerSample;
            public ushort blockAlign;
            public int byteRate;

            public int FileListOffset;
            public int LengthFieldOffset;

            public void PrintInfo()
            {
                Console.WriteLine(
                    $"Name: {name}\n" +
                    $"  Sample rate: {sampleRate} Hz\n" +
                    $"  Channels: {channels}\n" +
                    $"  Bits per sample: {bitsPerSample}\n" +
                    $"  Block alignment: {blockAlign} bytes\n" +
                    $"  Byte rate: {byteRate} bytes/sec\n" +
                    $"  Raw data length: {data.Length} bytes");
            }

            public void Save(Stream stream)
            {
                uint numsamples = 22050;
                ushort numchannels = 1;
                ushort samplelength = 1;

                BinaryWriter wr = new BinaryWriter(stream);

                wr.Write(Encoding.ASCII.GetBytes("RIFF"));
                wr.Write(36 + data.Length);
                wr.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
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

        private byte[] _header;
        private byte[] _fileList;
        public List<SoundFile> soundFiles = new List<SoundFile>();

        public BNMFile(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var r = new BinaryReader(stream);

            // Preserve the first 44 bytes as the header template
            _header = r.ReadBytes(44);

            // Use the header to determine the file list size
            // size1 is at offset 20 (0x14)
            int size1 = BitConverter.ToInt32(_header, 20);
            int size2 = BitConverter.ToInt32(_header, 24);

            _fileList = r.ReadBytes(size1 - 44);

            // Skip to the start of the audio data section
            int dataPos = size1;
            if (size2 - size1 > 0)
            {
                stream.Seek(size2, SeekOrigin.Begin);
            } else
            {
                stream.Seek(dataPos, SeekOrigin.Begin);
            }

            // Parse the file list to map sounds
            using var mr = new BinaryReader(new MemoryStream(_fileList));
            int eventCount = BitConverter.ToInt32(_header, 8);
            int fileCount = BitConverter.ToInt32(_header, 16);

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
                            FileListOffset = currentEntryOffset,
                            LengthFieldOffset = lengthOffset,
                            data = r.ReadBytes(length)
                        });
                    }
                }
            }
        }

        // public BNMFile(byte[] data)
        // {
        //     using var stream = new MemoryStream(data);
        //     using var r = new BinaryReader(stream);
        //
        //     // 1. Preserve Header
        //     _header = r.ReadBytes(44);
        //
        //     int size1 = BitConverter.ToInt32(_header, 20); // Offset to end of File List
        //     int size2 = BitConverter.ToInt32(_header, 24); // Total File Size
        //
        //     // 2. Read File List
        //     _fileList = r.ReadBytes(size1 - 44);
        //
        //     // 3. Position stream for audio data
        //     // The audio data starts immediately after the file list (at size1).
        //     // We seek to size1 to ensure we are at the start of the audio block,
        //     // regardless of any previous reads.
        //     stream.Seek(size1, SeekOrigin.Begin);
        //
        //     // 4. Parse File List to index sounds
        //     using var mr = new BinaryReader(new MemoryStream(_fileList));
        //     int eventCount = BitConverter.ToInt32(_header, 8);
        //     int fileCount = BitConverter.ToInt32(_header, 16);
        //
        //     if (eventCount > 0)
        //     {
        //         for (int i = 0; i < eventCount; i++) mr.ReadBytes(32);
        //     }
        //
        //     if (fileCount > 0)
        //     {
        //         for (int i = 0; i < fileCount; i++)
        //         {
        //             int currentEntryOffset = (int)mr.BaseStream.Position;
        //
        //             var id = mr.ReadByte();
        //             mr.ReadBytes(3);
        //             int type = mr.ReadInt32();
        //             mr.ReadBytes(4);
        //
        //             int lengthOffset = (int)mr.BaseStream.Position;
        //             int length = mr.ReadInt32();
        //
        //             if (type == 0xA)
        //             {
        //                 length = mr.ReadInt32();
        //                 mr.ReadBytes(40);
        //             } else
        //             {
        //                 mr.ReadBytes(44);
        //             }
        //
        //             var sampleRate = mr.ReadInt32();
        //             mr.ReadBytes(8);
        //             var nameBytes = mr.ReadBytes(20);
        //             var name = Encoding.ASCII.GetString(nameBytes).Split('\0')[0];
        //
        //             if (name.Length == 0) name = "UNKNOWN_TYPE";
        //             if (name.Contains(".apm")) length = 0;
        //
        //             if (type == 1 && !soundFiles.Any(f => f.name == name))
        //             {
        //                 // Read the actual audio data from the main stream 'r'
        //                 // Since we seeked to size1, this will now read the correct bytes.
        //                 soundFiles.Add(new SoundFile {
        //                     name = name,
        //                     sampleRate = sampleRate,
        //                     length = length,
        //                     FileListOffset = currentEntryOffset,
        //                     LengthFieldOffset = lengthOffset,
        //                     data = r.ReadBytes(length)
        //                 });
        //             }
        //         }
        //     }
        // }

        /// <summary>
        /// Rebuilds the BNM file. 
        /// replacementFiles: Dictionary where key is sound name and value is path to replacement .wav
        /// </summary>
        public byte[] SaveBNM(Dictionary<string, string> replacementFiles = null)
        {
            var updatedFileList = (byte[])_fileList.Clone();
            var audioDataStream = new MemoryStream();

            foreach (var sound in soundFiles)
            {
                byte[] finalData = sound.data;

                if (replacementFiles != null &&
                    replacementFiles.TryGetValue(sound.name, out string wavPath))
                {
                    finalData = ExtractRawPcmFromWav(wavPath);

                    Console.WriteLine(
                        $"Replacement: {wavPath}, extracted {finalData.Length} bytes");
                }

                Console.WriteLine(
                    $"BNM entry: {sound.name}, original {sound.data.Length} bytes, " +
                    $"final {finalData.Length} bytes");


                // Update the length in the file list binary
                byte[] lenBytes = BitConverter.GetBytes(finalData.Length);
                Buffer.BlockCopy(lenBytes, 0, updatedFileList, sound.LengthFieldOffset, 4);

                audioDataStream.Write(finalData, 0, finalData.Length);
            }

            // Update Header size fields
            byte[] updatedHeader = (byte[])_header.Clone();
            // int totalSize = 44 + updatedFileList.Length + (int)audioDataStream.Length;

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

        private byte[] ExtractRawPcmFromWav(string path)
        {
            using var fs = File.OpenRead(path);
            using var reader = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

            // RIFF
            string riffId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (riffId != "RIFF")
                throw new InvalidDataException("Not a RIFF file.");

            uint riffSize = reader.ReadUInt32();

            // WAVE
            string waveId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (waveId != "WAVE")
                throw new InvalidDataException("Not a WAVE file.");

            while (fs.Position + 8 <= fs.Length)
            {
                string chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
                uint chunkSize = reader.ReadUInt32();

                if (chunkSize > fs.Length - fs.Position)
                    throw new InvalidDataException(
                        $"Invalid {chunkId} chunk size: {chunkSize}.");

                if (chunkId == "data")
                {
                    return reader.ReadBytes(checked((int)chunkSize));
                }

                // RIFF chunks are word-aligned. Skip a padding byte for odd sizes.
                fs.Seek(chunkSize + (chunkSize & 1), SeekOrigin.Current);
            }

            throw new InvalidDataException("No data chunk found in WAV file.");
        }

    }
}
