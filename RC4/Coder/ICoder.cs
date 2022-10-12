namespace RC4.Coder;

public interface ICoder
{
    //Метод кодирования/декодирования файла и записи результата в файл
    void CodeFile(byte[] key, string fileToCode, string fileToResult, Mode mode);

    //Метод кодирования потока и передачи результата в данный поток
    void Code(byte[] key, BufferedStream input, BufferedStream output, Mode mode);
}

public enum Mode
{
    Encode,
    Decode
}