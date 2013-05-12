using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SpringCardPCSC;

namespace TestAntenne
{

    class Program    
    {
        static SpringCardPCSC.SCardReader _reader;
        static SpringCardPCSC.SCardChannel _channel;
        static bool needReader;

        static void Main(string[] args)
        {
            needReader = true;
            while (true)
            {
                
                if (needReader)
                {
                    SearchForReader(); //Recherche du lecteur
                }
                            
                try
                {
                    //_channel = new SpringCardPCSC.SCardChannel(_reader);
                    //_channel.ShareMode = SpringCardPCSC.SCARD.SHARE_DIRECT;
                    //_channel.Protocol = SpringCardPCSC.SCARD.PROTOCOL_NONE;


                    //CardBuffer cb = new CardBuffer(new byte[] { 0xFF, 0x58, 0x20, 0x01 });
                    //CardBuffer r2 = _channel.Control(cb);

                    //_channel.Connect();
                    //CardBuffer cb2 = _channel.Control(new CardBuffer("582200"));

                    //CWriteLine("Stop RF Field", ConsoleColor.Gray);

                    //SpringCardPCSC.CAPDU cmd2 = new SpringCardPCSC.CAPDU(new byte[] { 0xFF, 0x58, 0x21 });
                    //SpringCardPCSC.RAPDU result2 = Transmit(cmd2);
                    ////bool r1 = _channel.GetProductSerial();
                    //SpringCardPCSC.CAPDU cmd = new SpringCardPCSC.CAPDU(new byte[] { 0xFF, 0x58, 0x22, 0x00 });
                    //bool r = _channel.StopRF();
                    //SpringCardPCSC.RAPDU result = Transmit(cmd);
                    WaitCard();
                    //CWriteLine("Press a key to continue ", ConsoleColor.Yellow);
                    //Console.ReadKey();
                    //CWriteLine("Press a key to continue", ConsoleColor.Yellow);
                    //CWriteLine("Start RF Field", ConsoleColor.Gray);
                    //cmd = new SpringCardPCSC.CAPDU(new byte[] { 0xFF, 0x58, 0x23, 0x00 });
                    ////result = Transmit(cmd);
                    //CWriteLine("Press a key to continue", ConsoleColor.Yellow);
                    //Console.ReadKey();
                    //needReader = false;
                    //WaitCard();
                    
                    //Waiting for the card to be removed
                    CWriteLine("Remove the card ...", ConsoleColor.Yellow);
                    while (_reader.CardPresent)
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception e)
                {
                    CWriteLine("Erreur détectée : " + e.Message, ConsoleColor.Red);
                    needReader = true;
                }
            }
            
            CWriteLine("Appuyer sur une touche pour continuer ...", ConsoleColor.Yellow);
            Console.ReadKey();
        }

        static void CWriteLine(string s, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static bool SearchForReader()
        {
           int counter = 0;
            while (counter <= 50)
            {
                CWriteLine("Initialisation du lecteur (" + counter + ")",ConsoleColor.Gray);
                if (!GetReader())
                {
                    if (counter == 50)
                    {
                        Console.WriteLine("Pas de lecteur détecté, continuer (O/N) ? ");
                        ConsoleKeyInfo key = Console.ReadKey();
                        if (key.KeyChar == 'N' || key.KeyChar == 'n') return false;
                        counter = 0;
                    }
                    //else counter ++;
                }
                else
                {
                    CWriteLine("Lecteur détecté : " + _reader.Name,ConsoleColor.Gray);
                    return true;
                }
                counter++;
                Thread.Sleep(1000);
            }
            return false;
        }
        static bool GetReader()
        {
            string[] readerList = SpringCardPCSC.SCARD.Readers;

            if (readerList.Count() > 0)
            {
                _reader = new SpringCardPCSC.SCardReader(readerList[0]);
                return true;
            }

            return false;

        }
        static void WaitCard()
        {
            CWriteLine("Attente d'une carte ...",ConsoleColor.Gray);
            while (!_reader.CardPresent)
            {
                
                if (_reader.StatusAsString.Equals("UNAWARE")) {
                    CWriteLine(". => " + _reader.StatusAsString, ConsoleColor.Blue);
                }
                else {
                    CWriteLine(". => " + _reader.StatusAsString, ConsoleColor.Gray);
                }
                Thread.Sleep(500);
            }

            string atr = _reader.CardAtr.AsString();
            CWriteLine("Card Detectée : " + atr,ConsoleColor.Gray);
            
            CWriteLine("Serial : " + GetSerial(), ConsoleColor.Green);
            if (atr.Contains("3B8F8001804F0CA000000306030003"))
            {
                try
                {
                    CWriteLine("Block 0 : " + GetBlock(0), ConsoleColor.Green);
                    CWriteLine("Block 1 : " + GetBlock(1), ConsoleColor.Green);
                    CWriteLine("Block 2 : " + GetBlock(2), ConsoleColor.Green);
                }
                catch (ApplicationException ex)
                {
                    CWriteLine("Erreur : " + ex.Message, ConsoleColor.Red);
                }
            }
            else
            {
                CWriteLine("Carte non UltraLight..pas de lecture de la mémoire", ConsoleColor.DarkMagenta);
            }
        }
        static string GetSerial()
        {
            //Connect the channel
            if (!_channel.Connected)
            {
                if (!_channel.Connect())
                {
                    throw new ApplicationException("connection to card failed");
                }
            }
            
            SpringCardPCSC.CAPDU cmd = new SpringCardPCSC.CAPDU(new byte[] { 0xFF, 0xCA, 0x00, 0x00 });
            SpringCardPCSC.RAPDU result = _channel.Transmit(cmd);
            if (result.SWString.Equals("Success"))
            {
                return BitConverter.ToString(result.GetBytes());
            }
            else
            {
                throw new ApplicationException("Erreur de lecture du numéro de série");
            }
        }
        static string GetBlock(int blockNumber)
        {
            //If not connected
            if (!_channel.Connected)
            {
                if (!_channel.Connect())
                {
                    throw new ApplicationException("connection to card failed");
                }
            }

            SpringCardPCSC.CAPDU cmd = new SpringCardPCSC.CAPDU(new byte[] { 0xFF, 0xB0, 0x00, (byte)blockNumber });
            SpringCardPCSC.RAPDU result = _channel.Transmit(cmd);
            if (result.SWString.Equals("Success"))
            {
                return BitConverter.ToString(result.GetBytes());
            }
            else
            {
                throw new ApplicationException("Erreur de lecture du block " + blockNumber.ToString());
            }
        }
        static SpringCardPCSC.RAPDU Transmit(SpringCardPCSC.CAPDU cmd)
        {
            //Connect the channel
            //if (!_channel.Connected)
            //{
            //    if (!_channel.Connect())
            //    {
            //        throw new ApplicationException("connection to card failed");
            //    }
            //}
            return _channel.Transmit(cmd);

        }
    }
}
