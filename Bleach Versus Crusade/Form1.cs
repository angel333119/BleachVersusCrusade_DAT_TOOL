using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bleach_Versus_Crusade
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo DAT|*.DAT|Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha um arquivo DAT do jogo Bleach: Versus Crusade...";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    using (FileStream stream = File.Open(file, FileMode.Open))
                    {
                        BinaryReader br = new BinaryReader(stream);
                        BinaryWriter bw = new BinaryWriter(stream);

                        long tamanhoarquivo = new FileInfo(file).Length;

                        uint quantidadedeponteiros = Converterendian.bigendian32(br.ReadUInt32()) / 4;

                        br.BaseStream.Seek(0, SeekOrigin.Begin);

                        uint[] ponteiro = new uint[quantidadedeponteiros];

                        for (int i = 0; i < quantidadedeponteiros; i++)
                        {
                            ponteiro[i] = Converterendian.bigendian32(br.ReadUInt32());
                        }

                        uint[] tamanhodotexto = new uint[quantidadedeponteiros];

                        string todosostextos = "";

                        for (int i = 0; i < quantidadedeponteiros; i++)
                        {
                            br.BaseStream.Seek(ponteiro[i], SeekOrigin.Begin);

                            if (i < quantidadedeponteiros - 1)
                            {
                                tamanhodotexto[i] = ponteiro[i + 1] - ponteiro[i];
                            }
                            else
                            {
                                tamanhodotexto[i] = (uint)(tamanhoarquivo - ponteiro[i]);
                            }

                            byte[] bytestext = new byte[tamanhodotexto[i]];

                            for (int j = 0; j < tamanhodotexto[i]; j++)
                            {
                                bytestext[j] = br.ReadByte();
                            }
                            
                            string textodecodificado = Encoding.BigEndianUnicode.GetString(bytestext);

                            if (bytestext.SequenceEqual(new byte[] { 48, 0, 0, 0 }))
                            {
                                textodecodificado = "<48><00>";
                            }

                            todosostextos += textodecodificado.Replace("\0", String.Empty) + "\r\n";

                            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt", todosostextos);

                        }
                    }
                }
                MessageBox.Show("Texto extraido!", "AVISO!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Arquivo DAT|*.DAT|Todos os arquivos (*.*)|*.*";
            openFileDialog1.Title = "Escolha um arquivo DAT do jogo Bleach: Versus Crusade...";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    FileInfo dump = new FileInfo(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt");

                    if (dump.Exists)
                    {
                        using (FileStream stream = File.Open(file, FileMode.Open))
                        {
                            BinaryReader br = new BinaryReader(stream);
                            BinaryWriter bw = new BinaryWriter(stream);

                            var txt = File.ReadLines(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".txt");

                            long filesize = new FileInfo(file).Length;

                            uint primeiroponteiro = Converterendian.bigendian32(br.ReadUInt32());

                            stream.SetLength(primeiroponteiro);

                            int contador = 0;

                            uint novoponteiro = primeiroponteiro;

                            try
                            {
                                foreach (var linha in txt)
                                {
                                    bw.BaseStream.Seek(4 * contador, SeekOrigin.Begin);

                                    bw.Write(Converterendian.bigendian32(novoponteiro));

                                    bw.BaseStream.Seek(novoponteiro, SeekOrigin.Begin);

                                    byte[] bytes = new byte[0];

                                    if (linha == "<48><00>")
                                    {
                                        string hex = "3000";
                                        hex = hex.Replace("<", "").Replace(">", "");
                                        bytes = new byte[2];
                                        bytes[0] = Convert.ToByte(hex.Substring(0, 2), 16);
                                        bytes[1] = Convert.ToByte(hex.Substring(2, 2), 16);
                                    }
                                    else
                                    {
                                        bytes = Encoding.BigEndianUnicode.GetBytes(linha);
                                    }                                    

                                    bw.BaseStream.Seek(novoponteiro, SeekOrigin.Begin);

                                    if (string.IsNullOrWhiteSpace(linha))
                                    {
                                        bw.Write((short)0);
                                    }
                                    else
                                    {
                                        bw.Write(bytes);
                                    }

                                    novoponteiro = novoponteiro + (uint)bytes.Length + 2;

                                    contador++;
                                }
                            }
                            catch (System.IndexOutOfRangeException)
                            {

                            }
                            bw.Write((Int16)0);
                        }
                    }
                    else
                    {
                        MessageBox.Show("O arquivo TXT não foi encontrado.\n\nO arquivo TXT deve estar na mesma pasta do arquivo DAT.", "AVISO!");
                    }
                }
                MessageBox.Show("Texto inserido.", "AVISO!");
            }
        }
    }
}
