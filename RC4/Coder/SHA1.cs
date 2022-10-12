namespace RC4.Coder;

//Класс для хэширования файла
//Алгоритм 
public class Sha1
{
    //Размер одного блока текста в бит
    private const int sizeOfBlock = 512;
    //Размер одного символа в бит
    private const int sizeOfSymbol = 8;
    //Размер блока под количество бит в исходном файле(конец хэша)
    private const int sizeOfEnd = 64;

    private UInt32 A = 0x67452301;
    private UInt32 B = 0xEFCDAB89;
    private UInt32 C = 0x98BADCFE;
    private UInt32 D = 0x10325476;
    private UInt32 E = 0xC3D2E1F0;

    public void Hash(string filein, string fileout)
    {
        BufferedStream inputStream = new BufferedStream(new FileStream(filein, FileMode.Open));
        BufferedStream outStream = new BufferedStream(new FileStream(fileout, FileMode.OpenOrCreate));

        byte[] buffer = new byte[sizeOfBlock / sizeOfSymbol];

        UInt64 sizeOfFile = 0;
        while (true)
        {
            int countReaded = inputStream.Read(buffer, 0, sizeOfBlock / sizeOfSymbol);
            if (countReaded == 0)
            {
                break;
            }

            if (countReaded < sizeOfBlock / sizeOfSymbol)
            {
                byte[] newBuffer = new byte[countReaded];
                for (int i = 0; i < countReaded; ++i)
                {
                    newBuffer[i] = buffer[i];
                }
                IterateBlock(newBuffer, ref sizeOfFile);
                break;
            }
            
            IterateBlock(buffer, ref sizeOfFile);
        }

        byte[] outBuffer = fromUint32(A);
        outStream.Write(outBuffer);
        outBuffer = fromUint32(B);
        outStream.Write(outBuffer);
        outBuffer = fromUint32(C);
        outStream.Write(outBuffer);
        outBuffer = fromUint32(D);
        outStream.Write(outBuffer);
        outBuffer = fromUint32(E);
        outStream.Write(outBuffer);
        outStream.Flush();
        outStream.Close();
        inputStream.Close();
    }

    private void IterateBlock(
        byte[] block, 
        ref UInt64 currentSizeOfFile
        )
    {
        if (block.Length < sizeOfBlock / sizeOfSymbol)
        {
            currentSizeOfFile += (UInt32)(block.Length);
            block = ToRightEndBlocks(block, currentSizeOfFile);
            if (block.Length == sizeOfBlock / sizeOfSymbol)
            {
                IterateBlock(block, ref currentSizeOfFile);
            }

            if (block.Length == sizeOfBlock / sizeOfSymbol * 2)
            {
                byte[] block1 = new byte[sizeOfBlock / sizeOfSymbol];
                byte[] block2 = new byte[sizeOfBlock / sizeOfSymbol];

                for (int i = 0; i < sizeOfBlock / sizeOfSymbol; ++i)
                {
                    block1[i] = block[i];
                    block2[i] = block[i + sizeOfBlock / sizeOfSymbol];
                }
                IterateBlock(block1, ref currentSizeOfFile);
                IterateBlock(block2, ref currentSizeOfFile);
            }
            return;
        }

        UInt32[] uintblock = FromByteArrayToUInt32(block);
        currentSizeOfFile = currentSizeOfFile + (ulong)(block.Length * sizeOfSymbol);

        for (int i = 16; i < 80; ++i)
        {
            uintblock = 
                uintblock.Append(
                    Rotl(uintblock[i - 3] ^ uintblock[i - 8] ^ uintblock[i - 14] ^ uintblock[i - 16], 
                        1
                        )
                    );
        }


        UInt32 a = A;
        UInt32 b = B;
        UInt32 c = C;
        UInt32 d = D;
        UInt32 e = E;
        for (int t = 0; t < 80; ++t)
        {
            UInt32 temp = Rotl(a, 5) + Ft(b, c, d, t) + e + uintblock[t] + Kt(t);
            e = d;
            d = c;
            c = Rotl(b, 30);
            b = a;
            a = temp;
        }

        A += a;
        B += b;
        C += c;
        D += d;
        E += e;
    }

    //Преобразование массива байт в массив UInt32 чисел (размер входного массива должен быть кратным 4)
    UInt32[] FromByteArrayToUInt32(byte[] value)
    {
        UInt32[] result = Array.Empty<uint>();
        int j = 0;
        while (value.Length > j * 4)
        {
            UInt32 current = 0;
            for (int i = 0; i < 4; ++i)
            {
                current = current * 256 + value[j * 4 + i];
            }

            result = result.Append(current);
            j++;
        }

        return result;
    }

    //Функция дополнения блока с размером менее 512 бит до одного или двух блоков размерами 512 бит
    private byte[] ToRightEndBlocks(byte[] block, UInt64 currentSizeOfFile)
    {
        byte[] h = fromUint64(currentSizeOfFile);
        
        if (block.Length * sizeOfSymbol < sizeOfBlock - sizeOfEnd)
        {
            block = block.Append((byte)0b10000000);
            while (block.Length * sizeOfSymbol < sizeOfBlock - sizeOfEnd)
            {
                block = block.Append((byte)0b00000000);
            }

            foreach (byte s in h)
            {
                block = block.Append(s);
            }

            return block;
        }
        else
        {
            byte[] suffix = new byte[(sizeOfBlock - sizeOfEnd) / sizeOfSymbol];
            block = block.Append((byte)128);
            while (block.Length * sizeOfSymbol < sizeOfBlock)
            {
                block = block.Append((byte)0);
            }
            foreach (byte s in h)
            {
                suffix = suffix.Append(s);
            }
            foreach (byte s in suffix)
            {
                block = block.Append(s);
            }
            return block;
        }
    }

    private UInt32 Kt(int t)
    {
        if (t <= 19 && t >= 0)
        {
            return 0x5A827999;
        }
        if (t <= 39 && t >= 20)
        {
            return 0x6ED9EBA1;
        }
        if (t <= 59 && t >= 40)
        {
            return 0x8F1BBCDC;
        }
        if (t <= 79 && t >= 60)
        {
            return 0xCA62C1D6;
        }
        return 0;
    }

    private UInt32 Ft(UInt32 m, UInt32 l, UInt32 k, int t)
    {
        if (t <= 19 && t >= 0)
        {
            return ((m & l)|((~m) & k));
        }
        if (t <= 39 && t >= 20)
        {
            return (m ^ l ^ k);
        }
        if (t <= 59 && t >= 40)
        {
            return (m & l)|(m & k)|(l & k);
        }
        if (t <= 79 && t >= 60)
        {
            return (m ^ l ^ k);
        }
        return 0;
    }
    
    //Циклический сдвиг числа влево
    private UInt32 Rotl(UInt32 value, int bits)
    {
        if (bits == 0) return value;
    
        UInt32 right = value >> 1;
        if ((32 - bits) > 1)
        {
            right &= 0x7FFFFFFF;
            right >>= 32 - bits - 1;
        }
        return value << bits | right;
    }
    
    //Функция преобразующая 64-битное числов массив байт
    private byte[] fromUint64(UInt64 value)
    {
        byte h8 = (byte)(value % 256);
        value /= 256;
        byte h7 = (byte)(value % 256);
        value /= 256;
        byte h6 = (byte)(value % 256);
        value /= 256;
        byte h5 = (byte)(value % 256);
        value /= 256;
        byte h4 = (byte)(value % 256);
        value /= 256;
        byte h3 = (byte)(value % 256);
        value /= 256;
        byte h2 = (byte)(value % 256);
        value /= 256;
        byte h1 = (byte)(value % 256);

        byte[] result = Array.Empty<byte>();
        result = result.Append(h1);
        result = result.Append(h2);
        result = result.Append(h3);
        result = result.Append(h4);
        result = result.Append(h5);
        result = result.Append(h6);
        result = result.Append(h7);
        result = result.Append(h8);
        return result;
    }
    
    //Функция преобразующая 32-битное числов массив байт
    private byte[] fromUint32(UInt32 value)
    {
        byte h4 = (byte)(value % 256);
        value /= 256;
        byte h3 = (byte)(value % 256);
        value /= 256;
        byte h2 = (byte)(value % 256);
        value /= 256;
        byte h1 = (byte)(value % 256);

        byte[] result = Array.Empty<byte>();
        result = result.Append(h1);
        result = result.Append(h2);
        result = result.Append(h3);
        result = result.Append(h4);
        return result;
    }
}

public static class Extensions
{
    public static T[] Append<T>(this T[] array, T item)
    {
        if (array == null) {
            return new T[] { item };
        }
        T[] result = new T[array.Length + 1];
        array.CopyTo(result, 0);
        result[array.Length] = item;
        return result;
    }
}