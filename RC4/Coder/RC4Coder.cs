namespace RC4.Coder;

public class Rc4Coder : ICoder
{
    public void CodeFile(byte[] key, string fileToCode, string fileToResult, Mode mode)
    {
        BufferedStream inputStream = new BufferedStream(new FileStream(fileToCode, FileMode.Open));
        BufferedStream outStream = new BufferedStream(new FileStream(fileToResult, FileMode.OpenOrCreate));
        Code(key, inputStream, outStream, mode);
        inputStream.Close();
        outStream.Close();
    }
    
    public void Code(byte[] key, BufferedStream input, BufferedStream output, Mode mode)
    {
        KeyStream keyStream = new KeyStream(key);
        
        while (true)
        {
            int readed = input.ReadByte();

            if (readed == -1)
            {
                output.Flush();
                break;
            }
            byte curByte = (byte)readed;
            curByte ^= keyStream.GetByte();
            output.WriteByte(curByte);
        }
    }
}


//Класс генератора ключевого потока по ключу, используется для неограниченного расширения полученного
//ключа, чтобы в дальнейшем xor-ить его с входным потоком данных.
//Использует генератор псевдослучайной последовательности
public class KeyStream
{
    //Перестановки всех байт от 0x00 до 0xFF
    private byte[] s = new byte[256];
    //Переменные счетчики
    private int x = 0;
    private int y = 0;
    
    //Инициализация ключевого потока ключом, используется алгоритм ключевого расписания
    public KeyStream(byte[] key)
    {
        int keyLength = key.Length;

        for (int i = 0; i < 256; ++i)
        {
            s[i] = (byte)i;
        }

        int j = 0;
        for (int i = 0; i < 256; ++i)
        {
            j = (j + s[i] + key[i % keyLength]) % 256;
            (s[i], s[j]) = (s[j], s[i]);
        }
    }

    //Метод, который выплевывает очередной байт сгенерированного ключа,
    //используя генератор псевдослучайной последовательности
    public byte GetByte()
    {
        x = (x + 1) % 256;
        y = (y + s[x]) % 256;

        (s[x], s[y]) = (s[y], s[x]);

        return s[(s[x] + s[y]) % 256];
    }
}