namespace RC4.Coder;

public class DesCoder : ICoder
{
    private const int sizeOfBlock = 64; // Размер одного блока в бит
    private const int sizeOfChar = 8; //Размер одного символа в UTF8 в бит
    private const int countOfSymbolByBlock = sizeOfBlock / sizeOfChar;

    private const int shiftKey = 2; // Сдвиг ключа

    private const int quantityOfRounds = 16; // Количество раундов
    
    
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
        // Приводим размер ключа к размеру половины блока
        key = CorrectKeyWord(key, countOfSymbolByBlock / 2);

        byte[] currentBlock = new byte[countOfSymbolByBlock];
        while (true)
        {
            int countOfReaded = input.Read(currentBlock);
            if (countOfReaded == 0)
            {
                break;
            }

            if (countOfReaded < countOfSymbolByBlock)
            {
                currentBlock = ByteArrToRightLength(currentBlock);
            }
            switch (mode)
            {
                case Mode.Encode:
                {
                    for (int i = 0; i < quantityOfRounds; ++i)
                    {
                        currentBlock = EncodeDES_One_Round(currentBlock, key);
                    }
                    break;
                }
                case Mode.Decode:
                {
                    for (int i = 0; i < quantityOfRounds; ++i)
                    {
                        currentBlock = DecodeDES_One_Round(currentBlock, key);
                    }
                    break;
                }
            }
            output.Write(currentBlock);
            key = KeyToNextRound(key);
        }
        output.Flush();
    }

    //Метод доводящий размер блока до нужного размера (Дописываем в конец символ @)
    private byte[] ByteArrToRightLength(byte[] input)
    {
        while (input.Length < countOfSymbolByBlock)
        {
            input.Append((byte)64);
        }

        return input;
    }

    //Метод доводяший размер ключа до нужного размера (Дописываем в конец символ 0)
    private byte[] CorrectKeyWord(byte[] key, int lengthKey)
    {
        if (key.Length > lengthKey)
            Array.Resize<byte>(ref key, lengthKey);
        else
            while (key.Length < lengthKey)
                key.Append((byte)48);
 
        return key;
    }

    //Один раунд кодирования алгоритмом DES
    private byte[] EncodeDES_One_Round(byte[] input, byte[] key)
    {
        byte[] L = new byte[input.Length / 2];
        byte[] R = new byte[input.Length / 2];
        for (int i = 0; i < input.Length / 2; ++i)
        {
            L[i] = input[i];
            R[i] = input[i + input.Length / 2];
        }

        byte[] result = new byte[input.Length];
        for (int i = 0; i < input.Length / 2; ++i)
        {
            result[i] = R[i];
            result[i + input.Length / 2] = (byte)(L[i] ^ f(R[i], key[i]));
        }
        return result;
    }
    
    //Один раунд декодирования алгоритмом DES
    private byte[] DecodeDES_One_Round(byte[] input, byte[] key)
    {
        byte[] L = new byte[input.Length / 2];
        byte[] R = new byte[input.Length / 2];
        for (int i = 0; i < input.Length / 2; ++i)
        {
            L[i] = input[i];
            R[i] = input[i + input.Length / 2];
        }
        
        byte[] result = new byte[input.Length];
        for (int i = 0; i < input.Length / 2; ++i)
        {
            result[i] = (byte)(f(L[i], key[i]) ^ R[i]);
            result[i + input.Length / 2] = L[i];
        }
        return result;
    }

    //Шифрующая функция, в ее качестве тут выбран также XOR
    private byte f(byte s1, byte s2)
    {
        return (byte)(s1 ^ s2);
    }
    
    //Функция циклического сдвига ключа
    private byte[] KeyToNextRound(byte[] key)
    {
        for (int i = 0; i < shiftKey; ++i)
        {
            key[^1] = key[0];
            for (int j = 0; j < key.Length - 1; ++j)
            {
                key[j] = key[j + 1];
            }
        }
        return key;
    }
}