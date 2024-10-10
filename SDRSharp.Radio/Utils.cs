namespace SDRSharp.Radio
{
    public static class Utils
    {
        public unsafe static void Memcpy(void* dest, void* src, int len)
        {
            Buffer.MemoryCopy(src, dest, len, len);
        }
    }
}
