package ccutil;

public class Checksum32 {
    protected final int char_offset;
    protected int a;
    protected int b;
    public int k;
    protected int l;
    public byte[] block;
    protected int new_index;
    protected byte[] new_block;

    public Checksum32 (int char_offset) {
        this.char_offset = char_offset;
        a = b = 0;
        k = 0;
    }
    public Checksum32() {
        this (31);
    }
    public int getValue() {
        return (a & 0xffff) | (b << 16);
    }
    public void reset() {
        k = 0;
        a = b = 0;
        l = 0;
    }
    public void roll (byte bt) {
        a -= block[k] + char_offset;
        b -= l * (block[k] + char_offset);
        a += bt + char_offset;
        b += a;
        block[k] = bt;
        k++;
        if (k == l) {
            k = 0;
        }
    }
    public void trim() {
        a -= block[k % block.length] + char_offset;
        b -= l * (block[k % block.length] + char_offset);
        k++;
        l--;
    }
    public void check (byte[] buf, int off, int len) {
        block = new byte[len];
        System.arraycopy (buf, off, block, 0, len);
        reset();
        l = block.length;
        int i;
        for (i = 0; i < block.length - 4; i += 4) {
            b += 4 * (a + block[i]) + 3 * block[i + 1] +
                    2 * block[i + 2] + block[i + 3] + 10 * char_offset;
            a += block[i] + block[i + 1] + block[i + 2]
                    + block[i + 3] + 4 * char_offset;
        }
        for (; i < block.length; i++) {
            a += block[i] + char_offset;
            b += a;
        }
    }
    public byte[] getBlock(){
        byte[] ret=new byte[block.length];
        int kk=k;
        int pos=0;
        if(kk==0)kk=block.length;
        for(int i=kk;i<block.length;i++)ret[pos++]=block[i];
        for(int i=0;i<kk;i++)ret[pos++]=block[i];
        return ret;
    }
}
