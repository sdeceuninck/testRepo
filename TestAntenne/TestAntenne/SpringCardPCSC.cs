/**h* SpringCard/PCSC
 *
 * NAME
 *   PCSC
 * 
 * DESCRIPTION
 *   SpringCard's wrapper for PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2012 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D and Emilie.C / SpringCard
 *
 * HISTORY  
 *   ECL ../../2009 : early drafts
 *   JDA 21/04/2010 : first official release
 *   JDA 20/11/2010 : improved the SCardChannel object: implemented SCardControl, exported the hCard
 *   JDA 24/01/2011 : added static DefaultReader and DefaultCardChannel to ease 'quick and dirty' development for simple applications
 *   JDA 25/01/2011 : added SCardChannel.Reconnect methods
 *   JDA 16/01/2012 : improved CardBuffer, CAPDU and RAPDU objects for robustness
 *   JDA 12/02/2012 : added the SCardReaderList object to monitor all the readers
 *   JDA 26/03/2012 : added SCARD_PCI_T0, SCARD_PCI_T1 and SCARD_PCI_RAW
 *   JDA 07/02/2012 : minor improvements
 *
 * PORTABILITY
 *   .NET on Win32 (not yet validated on Win64)
 *
 **/

using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SpringCardPCSC
{

/**c* SpringCardPCSC/SCARD
 *
 * NAME
 *   SCARD
 * 
 * DESCRIPTION
 *   Static class that gives access to PC/SC functions (SCard... provided by winscard.dll)
 *
 **/
  public abstract partial class SCARD
  {
    public static SCardReader DefaultReader = null;
    public static SCardChannel DefaultCardChannel = null;

#region Constants for parameters and status

    public const uint SCOPE_USER = 0;
    public const uint SCOPE_TERMINAL = 1;
    public const uint SCOPE_SYSTEM = 2;
    
    public const string ALL_READERS = "SCard$AllReaders\0\0";
    public const string DEFAULT_READERS = "SCard$DefaultReaders\0\0";
    public const string LOCAL_READERS = "SCard$LocalReaders\0\0";
    public const string SYSTEM_READERS = "SCard$SystemReaders\0\0";

    public const uint SHARE_EXCLUSIVE = 1;
    public const uint SHARE_SHARED = 2;
    public const uint SHARE_DIRECT = 3;

    public const uint PROTOCOL_NONE = 0;
    public const uint PROTOCOL_T0 = 1;
    public const uint PROTOCOL_T1 = 2;
    public const uint PROTOCOL_RAW = 4;

    public const uint LEAVE_CARD = 0; // Don't do anything special on close
    public const uint RESET_CARD = 1; // Reset the card on close
    public const uint UNPOWER_CARD = 2; // Power down the card on close
    public const uint EJECT_CARD = 3; // Eject the card on close    

    public const uint STATE_UNAWARE = 0x00000000;
    public const uint STATE_IGNORE = 0x00000001;
    public const uint STATE_CHANGED = 0x00000002;
    public const uint STATE_UNKNOWN = 0x00000004;
    public const uint STATE_UNAVAILABLE = 0x00000008;
    public const uint STATE_EMPTY = 0x00000010;
    public const uint STATE_PRESENT = 0x00000020;
    public const uint STATE_ATRMATCH = 0x00000040;
    public const uint STATE_EXCLUSIVE = 0x00000080;
    public const uint STATE_INUSE = 0x00000100;
    public const uint STATE_MUTE = 0x00000200;
    public const uint STATE_UNPOWERED = 0x00000400;

    public const uint IOCTL_CSB6_PCSC_ESCAPE = 0x00312000;
    public const uint IOCTL_MS_CCID_ESCAPE = 0x003136B0;

#endregion

#region Error codes
    public const uint S_SUCCESS = 0x00000000;
    public const uint F_INTERNAL_ERROR = 0x80100001;
    public const uint E_CANCELLED = 0x80100002;
    public const uint E_INVALID_HANDLE = 0x80100003;
    public const uint E_INVALID_PARAMETER = 0x80100004;
    public const uint E_INVALID_TARGET = 0x80100005;
    public const uint E_NO_MEMORY = 0x80100006;
    public const uint F_WAITED_TOO_LONG = 0x80100007;
    public const uint E_INSUFFICIENT_BUFFER = 0x80100008;
    public const uint E_UNKNOWN_READER = 0x80100009;
    public const uint E_TIMEOUT = 0x8010000A;
    public const uint E_SHARING_VIOLATION = 0x8010000B;
    public const uint E_NO_SMARTCARD = 0x8010000C;
    public const uint E_UNKNOWN_CARD = 0x8010000D;
    public const uint E_CANT_DISPOSE = 0x8010000E;
    public const uint E_PROTO_MISMATCH = 0x8010000F;
    public const uint E_NOT_READY = 0x80100010;
    public const uint E_INVALID_VALUE = 0x80100011;
    public const uint E_SYSTEM_CANCELLED = 0x80100012;
    public const uint F_COMM_ERROR = 0x80100013;
    public const uint F_UNKNOWN_ERROR = 0x80100014;
    public const uint E_INVALID_ATR = 0x80100015;
    public const uint E_NOT_TRANSACTED = 0x80100016;
    public const uint E_READER_UNAVAILABLE = 0x80100017;
    public const uint P_SHUTDOWN = 0x80100018;
    public const uint E_PCI_TOO_SMALL = 0x80100019;
    public const uint E_READER_UNSUPPORTED = 0x8010001A;
    public const uint E_DUPLICATE_READER = 0x8010001B;
    public const uint E_CARD_UNSUPPORTED = 0x8010001C;
    public const uint E_NO_SERVICE = 0x8010001D;
    public const uint E_SERVICE_STOPPED = 0x8010001E;
    public const uint E_UNEXPECTED = 0x8010001F;
    public const uint E_ICC_INSTALLATION = 0x80100020;
    public const uint E_ICC_CREATEORDER = 0x80100021;
    public const uint E_UNSUPPORTED_FEATURE = 0x80100022;
    public const uint E_DIR_NOT_FOUND = 0x80100023;
    public const uint E_FILE_NOT_FOUND = 0x80100024;
    public const uint E_NO_DIR = 0x80100025;
    public const uint E_NO_FILE = 0x80100026;
    public const uint E_NO_ACCESS = 0x80100027;
    public const uint E_WRITE_TOO_MANY = 0x80100028;
    public const uint E_BAD_SEEK = 0x80100029;
    public const uint E_INVALID_CHV = 0x8010002A;
    public const uint E_UNKNOWN_RES_MNG = 0x8010002B;
    public const uint E_NO_SUCH_CERTIFICATE = 0x8010002C;
    public const uint E_CERTIFICATE_UNAVAILABLE = 0x8010002D;
    public const uint E_NO_READERS_AVAILABLE = 0x8010002E;
    public const uint E_COMM_DATA_LOST = 0x8010002F;
    public const uint E_NO_KEY_CONTAINER = 0x80100030;
    public const uint W_UNSUPPORTED_CARD = 0x80100065;
    public const uint W_UNRESPONSIVE_CARD = 0x80100066;
    public const uint W_UNPOWERED_CARD = 0x80100067;
    public const uint W_RESET_CARD = 0x80100068;
    public const uint W_REMOVED_CARD = 0x80100069;
    public const uint W_SECURITY_VIOLATION = 0x8010006A;
    public const uint W_WRONG_CHV = 0x8010006B;
    public const uint W_CHV_BLOCKED = 0x8010006C;
    public const uint W_EOF = 0x8010006D;
    public const uint W_CANCELLED_BY_USER = 0x8010006E;
    public const uint W_CARD_NOT_AUTHENTICATED = 0x8010006F;
#endregion

#region Definition of the 'native' structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] public struct READERSTATE
    {
      internal string szReader;
      internal IntPtr pvUserData;
      internal uint dwCurrentState;
      internal uint dwEventState;
      internal uint cbAtr;
      [MarshalAs (UnmanagedType.ByValArray, SizeConst = 0x24, ArraySubType = UnmanagedType.U1)]
      internal byte[] rgbAtr;
    }
#endregion

#region The ugly SCARD_PCI_xx global variables

    [DllImport("kernel32.dll")]
    private extern static IntPtr LoadLibrary(string fileName) ;
    
    [DllImport("kernel32.dll")]
    private extern static void FreeLibrary(IntPtr handle) ;
    
    [DllImport("kernel32.dll")]
    private extern static IntPtr GetProcAddress(IntPtr handle, string procName);
    
    /* Get the address of SCARD_PCI_T0 from "Winscard.dll" */
    private static IntPtr _scard_pci_t0 = IntPtr.Zero;
    public static IntPtr PCI_T0()
    {
      if (_scard_pci_t0 == IntPtr.Zero)
      {
        IntPtr handle = LoadLibrary("Winscard.dll") ;
        _scard_pci_t0 = GetProcAddress(handle, "g_rgSCardT0Pci");
        FreeLibrary(handle) ;
      }
      return _scard_pci_t0;
    }

    /* Get the address of SCARD_PCI_T1 from "Winscard.dll" */
    private static IntPtr _scard_pci_t1 = IntPtr.Zero;
    public static IntPtr PCI_T1()
    {
      if (_scard_pci_t1 == IntPtr.Zero)
      {
        IntPtr handle = LoadLibrary("Winscard.dll") ;
        _scard_pci_t1 = GetProcAddress(handle, "g_rgSCardT1Pci");
        FreeLibrary(handle) ;
      }
      return _scard_pci_t1;
    }

    /* Get the address of SCARD_PCI_RAW from "Winscard.dll" */
    private static IntPtr _scard_pci_raw = IntPtr.Zero;
    public static IntPtr PCI_RAW()
    {
      if (_scard_pci_raw == IntPtr.Zero)
      {
        IntPtr handle = LoadLibrary("Winscard.dll") ;
        _scard_pci_raw = GetProcAddress(handle, "g_rgSCardRawPci");
        FreeLibrary(handle) ;
      }
      return _scard_pci_raw;
    }
#endregion
      
#region Static methods, provided by the 'native' WINSCARD library

/**f* SCARD/EstablishContext
 *
 * NAME
 *   SCARD.EstablishContext
 *
 * DESCRIPTION
 *   .NET wrapper for SCardEstablishContext
 *
 **/
    [DllImport("WinScard.dll", EntryPoint = "SCardEstablishContext")]
      public static extern uint EstablishContext(uint dwScope,
                                                 IntPtr nNotUsed1,
                                                 IntPtr nNotUsed2,
                                                 ref IntPtr phContext);

/**f* SCARD/ReleaseContext
 *
 * NAME
 *   SCARD.ReleaseContext
 *
 * DESCRIPTION
 *   .NET wrapper for SCardReleaseContext
 *
 **/
    [DllImport("WinScard.dll", EntryPoint = "SCardReleaseContext")]
      public static extern uint ReleaseContext(IntPtr Context);

/**f* SCARD/ListReaders
 *
 * NAME
 *   SCARD.ListReaders
 *
 * DESCRIPTION
 *   .NET wrapper for SCardListReaders (UNICODE implementation)
 *
 **/
    [DllImport
     ("winscard.dll", EntryPoint = "SCardListReadersW", SetLastError =
      true, CharSet =
      CharSet.Unicode)] public static extern uint ListReaders(IntPtr context,
                                                              string groups,
                                                              string readers,
                                                              ref uint size);

/**f* SCARD/GetStatusChange
 *
 * NAME
 *   SCARD.GetStatusChange
 *
 * DESCRIPTION
 *   .NET wrapper for SCardGetStatusChange (UNICODE implementation)
 *
 **/
    [DllImport
     ("winscard.dll", EntryPoint = "SCardGetStatusChangeW", CharSet =
      CharSet.
      Unicode)] public static extern uint GetStatusChange(IntPtr hContext,
                                                          uint dwTimeout,[In,
                                                                          Out,
                                                                          MarshalAs
                                                                          (UnmanagedType.
                                                                           LPArray,
                                                                           SizeParamIndex
                                                                           =
                                                                           3)]
                                                          SCARD.
                                                          READERSTATE
                                                          []rgReaderState,
                                                          uint cReaders);

/**f* SCARD/Connect
 *
 * NAME
 *   SCARD.Connect
 *
 * DESCRIPTION
 *   .NET wrapper for SCardConnect (UNICODE implementation)
 *
 **/
    [DllImport
     ("WinScard.dll", EntryPoint = "SCardConnectW", CharSet =
      CharSet.Unicode)] public static extern uint Connect(IntPtr hContext,
                                                          string cReaderName,
                                                          uint dwShareMode,
                                                          uint dwPrefProtocol,
                                                          ref IntPtr phCard,
                                                          ref uint
                                                          ActiveProtocol);

/**f* SCARD/Reconnect
 *
 * NAME
 *   SCARD.Reconnect
 *
 * DESCRIPTION
 *   .NET wrapper for SCardReconnect
 *
 **/
    [DllImport("winscard.dll", EntryPoint = "SCardReconnect")]
      public static extern uint Reconnect(IntPtr hCard,
                                          uint dwShareMode,
                                          uint dwPrefProtocol, uint swInit,
                                          ref uint ActiveProtocol);

/**f* SCARD/Disconnect
 *
 * NAME
 *   SCARD.Disconnect
 *
 * DESCRIPTION
 *   .NET wrapper for SCardDisconnect
 *
 **/
    [DllImport("WinScard.dll", EntryPoint = "SCardDisconnect")]
      public static extern uint Disconnect(IntPtr hCard, uint Disposition);

/**f* SCARD/Status
 *
 * NAME
 *   SCARD.Status
 *
 * DESCRIPTION
 *   .NET wrapper for SCardStatus (UNICODE version)
 *
 **/
    [DllImport
     ("winscard.dll", EntryPoint = "SCardStatusW", SetLastError =
      true, CharSet =
      CharSet.Unicode)] public static extern uint Status(IntPtr hCard,
                                                         IntPtr
                                                         mszReaderNames,
                                                         ref uint
                                                         pcchReaderLen,
                                                         ref uint readerState,
                                                         ref uint protocol,
                                                         [In,
                                                          Out]
                                                         byte[]atr_bytes,
                                                         ref uint atr_length);

/**f* SCARD/Transmit
 *
 * NAME
 *   SCARD.Transmit
 *
 * DESCRIPTION
 *   .NET wrapper for SCardTransmit
 *
 **/
    [DllImport
     ("winscard.dll", EntryPoint = "SCardTransmit", SetLastError =
      true)] public static extern uint Transmit(IntPtr hCard,
                                                IntPtr pioSendPci,
                                                byte[]pbSendBuffer,
                                                uint cbSendLength,
                                                IntPtr pioRecvPci,[In,
                                                                   Out]
                                                byte[]pbRecvBuffer,[In,
                                                                    Out] ref
                                                uint pcbRecvLength);

/**f* SCARD/Control
 *
 * NAME
 *   SCARD.Control
 *
 * DESCRIPTION
 *   .NET wrapper for SCardControl
 *
 **/
    [DllImport
     ("winscard.dll", EntryPoint = "SCardControl", SetLastError =
      true)] public static extern uint Control(IntPtr hCard, uint ctlCode,
                                               [In] byte[]pbSendBuffer,
                                               uint cbSendLength,[In,
                                                                  Out]
                                               byte[]pbRecvBuffer,
                                               uint RecvBuffsize,
                                               ref uint pcbRecvLength);
#endregion

#region Static methods - easy access to the list of readers
    public static string[] GetReaderList(IntPtr hContext, string Groups)
    {
      int i, j = 0;
      string s = "";
      uint rc;
      uint readers_size = 0;
      int readers_count = 0;

      rc = SCARD.ListReaders(hContext, Groups, null, ref readers_size);
      if (rc != SCARD.S_SUCCESS)
        return null;

      string readers_str = new string(' ', (int) readers_size);
      
      rc = SCARD.ListReaders(hContext, Groups, readers_str, ref readers_size);
      if (rc != SCARD.S_SUCCESS)
        return null;

      for (i = 0; i < readers_size; i++)
      {
        if (readers_str[i] == '\0')
        {
          if (i > 0)
            readers_count++;
          if (readers_str[i + 1] == '\0')
            break;
        }
      }
      
      string[] readers = new string[readers_count];

      if (readers_count > 0)
      {
        for (i = 0; i < readers_size; i++)
        {
          if (readers_str[i] == '\0')
          {
            readers[j++] = s;
            if (readers_str[i + 1] == '\0')
              break;
            s = "";
          } else
          {
            s = s + readers_str[i];
          }
        }
      }

      return readers;
    }
    
    public static string[] GetReaderList(IntPtr hContext)
    {
      return GetReaderList(hContext, null);
    }

    public static string[] GetReaderList(uint Scope, string Groups)
    {    
      IntPtr hContext = IntPtr.Zero;
      uint rc;

      rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
      if (rc != SCARD.S_SUCCESS)
        return null;

      string[] readers = GetReaderList(hContext, Groups);

      SCARD.ReleaseContext(hContext);

      return readers;
    }
    
    public static string[] GetReaderList()
    {
      return GetReaderList(SCARD.SCOPE_SYSTEM, null);
    }


/**f* SCARD/Readers
 *
 * NAME
 *   SCARD.Readers
 *
 * DESCRIPTION
 *   Provides the list of the connected PC/SC readers
 *
 * SYNOPSIS
 *   string[] SCARD.Readers
 *
 **/
    public static string[] Readers
    {
      get
      {
        return GetReaderList();
      }
    }
#endregion

#region Static methods - helpers to format status and errors

/**f* SCARD/ErrorToString
 *
 * NAME
 *   SCARD.ErrorToString
 *
 * DESCRIPTION
 *   Translate a PC/SC error code into a user-readable string
 *
 * SYNOPSIS
 *   string SCARD.ErrorToString( uint code );
 *
 **/
    public static string ErrorToString(uint code)
    {
      string r = "";
      try
      {
        r = (new Win32Exception((int) code)).Message;
      } catch (Exception)
      {
        
      }
      
      if (!r.Equals(""))
        return r;      
      
      switch (code)
      {
        case SCARD.S_SUCCESS:
          return "SCARD_S_SUCCESS";
        case SCARD.F_INTERNAL_ERROR:
          return "SCARD_F_INTERNAL_ERROR";
        case SCARD.E_CANCELLED:
          return "SCARD_E_CANCELLED";
        case SCARD.E_INVALID_HANDLE:
          return "SCARD_E_INVALID_HANDLE";
        case SCARD.E_INVALID_PARAMETER:
          return "SCARD_E_INVALID_PARAMETER";
        case SCARD.E_INVALID_TARGET:
          return "SCARD_E_INVALID_TARGET";
        case SCARD.E_NO_MEMORY:
          return "SCARD_E_NO_MEMORY";
        case SCARD.F_WAITED_TOO_LONG:
          return "SCARD_F_WAITED_TOO_LONG";
        case SCARD.E_INSUFFICIENT_BUFFER:
          return "SCARD_E_INSUFFICIENT_BUFFER";
        case SCARD.E_UNKNOWN_READER:
          return "SCARD_E_UNKNOWN_READER";
        case SCARD.E_TIMEOUT:
          return "SCARD_E_TIMEOUT";
        case SCARD.E_SHARING_VIOLATION:
          return "SCARD_E_SHARING_VIOLATION";
        case SCARD.E_NO_SMARTCARD:
          return "SCARD_E_NO_SMARTCARD";
        case SCARD.E_UNKNOWN_CARD:
          return "SCARD_E_UNKNOWN_CARD";
        case SCARD.E_CANT_DISPOSE:
          return "SCARD_E_CANT_DISPOSE";
        case SCARD.E_PROTO_MISMATCH:
          return "SCARD_E_PROTO_MISMATCH";
        case SCARD.E_NOT_READY:
          return "SCARD_E_NOT_READY";
        case SCARD.E_INVALID_VALUE:
          return "SCARD_E_INVALID_VALUE";
        case SCARD.E_SYSTEM_CANCELLED:
          return "SCARD_E_SYSTEM_CANCELLED";
        case SCARD.F_COMM_ERROR:
          return "SCARD_F_COMM_ERROR";
        case SCARD.F_UNKNOWN_ERROR:
          return "SCARD_F_UNKNOWN_ERROR";
        case SCARD.E_INVALID_ATR:
          return "SCARD_E_INVALID_ATR";
        case SCARD.E_NOT_TRANSACTED:
          return "SCARD_E_NOT_TRANSACTED";
        case SCARD.E_READER_UNAVAILABLE:
          return "SCARD_E_READER_UNAVAILABLE";
        case SCARD.P_SHUTDOWN:
          return "SCARD_P_SHUTDOWN";
        case SCARD.E_PCI_TOO_SMALL:
          return "SCARD_E_PCI_TOO_SMALL";
        case SCARD.E_READER_UNSUPPORTED:
          return "SCARD_E_READER_UNSUPPORTED";
        case SCARD.E_DUPLICATE_READER:
          return "SCARD_E_DUPLICATE_READER";
        case SCARD.E_CARD_UNSUPPORTED:
          return "SCARD_E_CARD_UNSUPPORTED";
        case SCARD.E_NO_SERVICE:
          return "SCARD_E_NO_SERVICE";
        case SCARD.E_SERVICE_STOPPED:
          return "SCARD_E_SERVICE_STOPPED";
        case SCARD.E_UNEXPECTED:
          return "SCARD_E_UNEXPECTED";
        case SCARD.E_ICC_INSTALLATION:
          return "SCARD_E_ICC_INSTALLATION";
        case SCARD.E_ICC_CREATEORDER:
          return "SCARD_E_ICC_CREATEORDER";
        case SCARD.E_UNSUPPORTED_FEATURE:
          return "SCARD_E_UNSUPPORTED_FEATURE";
        case SCARD.E_DIR_NOT_FOUND:
          return "SCARD_E_DIR_NOT_FOUND";
        case SCARD.E_FILE_NOT_FOUND:
          return "SCARD_E_FILE_NOT_FOUND";
        case SCARD.E_NO_DIR:
          return "SCARD_E_NO_DIR";
        case SCARD.E_NO_FILE:
          return "SCARD_E_NO_FILE";
        case SCARD.E_NO_ACCESS:
          return "SCARD_E_NO_ACCESS";
        case SCARD.E_WRITE_TOO_MANY:
          return "SCARD_E_WRITE_TOO_MANY";
        case SCARD.E_BAD_SEEK:
          return "SCARD_E_BAD_SEEK";
        case SCARD.E_INVALID_CHV:
          return "SCARD_E_INVALID_CHV";
        case SCARD.E_UNKNOWN_RES_MNG:
          return "SCARD_E_UNKNOWN_RES_MNG";
        case SCARD.E_NO_SUCH_CERTIFICATE:
          return "SCARD_E_NO_SUCH_CERTIFICATE";
        case SCARD.E_CERTIFICATE_UNAVAILABLE:
          return "SCARD_E_CERTIFICATE_UNAVAILABLE";
        case SCARD.E_NO_READERS_AVAILABLE:
          return "SCARD_E_NO_READERS_AVAILABLE";
        case SCARD.E_COMM_DATA_LOST:
          return "SCARD_E_COMM_DATA_LOST";
        case SCARD.E_NO_KEY_CONTAINER:
          return "SCARD_E_NO_KEY_CONTAINER";
          //case SCARD.E_SERVER_TOO_BUSY : return "SCARD_E_SERVER_TOO_BUSY";
        case SCARD.W_UNSUPPORTED_CARD:
          return "SCARD_W_UNSUPPORTED_CARD";
        case SCARD.W_UNRESPONSIVE_CARD:
          return "SCARD_W_UNRESPONSIVE_CARD";
        case SCARD.W_UNPOWERED_CARD:
          return "SCARD_W_UNPOWERED_CARD";
        case SCARD.W_RESET_CARD:
          return "SCARD_W_RESET_CARD";
        case SCARD.W_REMOVED_CARD:
          return "SCARD_W_REMOVED_CARD";
        case SCARD.W_SECURITY_VIOLATION:
          return "SCARD_W_SECURITY_VIOLATION";
        case SCARD.W_WRONG_CHV:
          return "SCARD_W_WRONG_CHV";
        case SCARD.W_CHV_BLOCKED:
          return "SCARD_W_CHV_BLOCKED";
        case SCARD.W_EOF:
          return "SCARD_W_EOF";
        case SCARD.W_CANCELLED_BY_USER:
          return "SCARD_W_CANCELLED_BY_USER";
        case SCARD.W_CARD_NOT_AUTHENTICATED:
          return "SCARD_W_CARD_NOT_AUTHENTICATED";
        default:          
          return r;
      }
    }

/**f* SCARD/ReaderStatusToString
 *
 * NAME
 *   SCARD.ReaderStatusToString
 *
 * DESCRIPTION
 *   Translate the Status of the reader into a user-readable string
 *
 * SYNOPSIS
 *   string SCARD.ReaderStatusToString( uint state );
 *
 **/
    public static string ReaderStatusToString(uint state)
    {
      string r = "";

      if (state == SCARD.STATE_UNAWARE)
        r += ",UNAWARE";

      if ((state & SCARD.STATE_EMPTY) != 0)
        r += ",EMPTY";
      if ((state & SCARD.STATE_PRESENT) != 0)
        r += ",PRESENT";
      if ((state & SCARD.STATE_MUTE) != 0)
        r += ",MUTE";
      if ((state & SCARD.STATE_UNPOWERED) != 0)
        r += ",UNPOWERED";
      if ((state & SCARD.STATE_ATRMATCH) != 0)
        r += ",ATRMATCH";
      if ((state & SCARD.STATE_EXCLUSIVE) != 0)
        r += ",EXCLUSIVE";
      if ((state & SCARD.STATE_INUSE) != 0)
        r += ",INUSE";

      if ((state & SCARD.STATE_IGNORE) != 0)
        r += ",IGNORE";
      if ((state & SCARD.STATE_UNKNOWN) != 0)
        r += ",UNKNOWN";
      if ((state & SCARD.STATE_UNAVAILABLE) != 0)
        r += ",UNAVAILABLE";

      if ((state & SCARD.STATE_CHANGED) != 0)
        r += ",CHANGED";

      if (r.Length >= 1)
        r = r.Substring(1);

      return r;
    }

/**f* SCARD/CardProtocolToString
 *
 * NAME
 *   SCARD.CardProtocolToString
 *
 * DESCRIPTION
 *   Translate the Protocol of the card into a user-readable string
 *
 * SYNOPSIS
 *   string SCARD.CardProtocolToString( uint protocol );
 *
 **/
    public static string CardProtocolToString(uint protocol)
    {
      if (protocol == SCARD.PROTOCOL_NONE)
        return "";      
      if (protocol == SCARD.PROTOCOL_T0)
        return "T=0";
      if (protocol == SCARD.PROTOCOL_T1)
        return "T=1";
      if (protocol == SCARD.PROTOCOL_RAW)
        return "RAW";

      return "Unknown";
    }

/**f* SCARD/CardShareModeToString
 *
 * NAME
 *   SCARD.CardShareModeToString
 *
 * DESCRIPTION
 *   Translate the Share Mode of the card into a user-readable string
 *
 * SYNOPSIS
 *   string SCARD.CardShareModeToString( uint share_mode );
 *
 **/
    public static string CardShareModeToString(uint share_mode)
    {
      if (share_mode == SCARD.SHARE_SHARED)
        return "SHARED";
      if (share_mode == SCARD.SHARE_EXCLUSIVE)
        return "EXCLUSIVE";
      if (share_mode == SCARD.SHARE_DIRECT)
        return "DIRECT";

      return "Unknown";
    }

/**f* SCARD/CardStatusWordsToString
 *
 * NAME
 *   SCARD.CardStatusWordsToString
 *
 * DESCRIPTION
 *   Translate the Status Word of the card into a user-readable string
 *
 * SYNOPSIS
 *   string SCARD.CardStatusWordsToString( byte SW1, byte SW2 );
 *   string SCARD.CardStatusWordsToString( ushort SW );
 *
 **/
    public static string CardStatusWordsToString(ushort SW)
    {
      byte SW1 = (byte) (SW / 0x0100);
      byte SW2 = (byte) (SW % 0x0100);

      return CardStatusWordsToString(SW1, SW2);
    }

    public static string CardStatusWordsToString(byte SW1, byte SW2)
    {
      switch (SW1)
      {
        case 0x60:
          return "null";

        case 0x61:
          return "Still " + SW2.ToString() +
            " bytes available. Use GET RESPONSE to access this data";

        case 0x62:
          switch (SW2)
          {
            case 0x81:
              return "Warning : returned data may be corrupted";
            case 0x82:
              return "Warning : EoF has been reached before ending";
            case 0x83:
              return "Warning : selected file invalidated";
            case 0x84:
              return "Warning : bad file control information format";
            default:
              return "Warning : state unchanged";
          }

        case 0x63:
          if ((SW2 >= 0xC0) && (SW2 <= 0xCF))
            return "Warning : counter value is " + SW2.ToString();

          if (SW2 == 81)
            return "Warning : file filled up with last write";
          return "Warning : state unchanged";

        case 0x64:
          return "Error : state unchanged";

        case 0x65:
          switch (SW2)
          {
            case 0x01:
              return "Memory failure, problem in writing the EEPROM";
            case 0x81:
              return "Error : memory failure";
            default:
              return "Error : state changed";
          }

        case 0x66:
          return "Security error";

        case 0x67:
          return "Check error - incorrect byte length";

        case 0x68:
          switch (SW2)
          {
            case 0x81:
              return "Check error - logical channel not supported";
            case 0x82:
              return "Check error : secure messaging not supported";
            default:
              return "Check error : request function not supported";
          }

        case 0x69:
          switch (SW2)
          {
            case 0x81:
              return "Check error : command incompatible with file structure";
            case 0x82:
              return "Check error : security status not statisfied";
            case 0x83:
              return "Check error : authentication method blocked";
            case 0x84:
              return "Check error : referenced data invalidated";
            case 0x85:
              return "Check error : conditions of use not satisfied";
            case 0x86:
              return "Check error : command not allowed (no current EF)";
            case 0x87:
              return "Check error : Expected SM data objects missing";
            case 0x88:
              return "Check error : SM data objects incorrect ";
            default:
              return
                "Unknow command, most probably erroneous typing, protocol violation or incorrect format";
          }

        case 0x6A:
          switch (SW2)
          {
            case 0x00:
              return "Check error : P1 or P2 incorrect";
            case 0x80:
              return "Check error : parameters in data field incorrect";
            case 0x81:
              return "Check error : function not supported";
            case 0x82:
              return "Check error : file not found";
            case 0x83:
              return "Check error : record not found";
            case 0x84:
              return "Check error : insufficient memory space in this file";
            case 0x85:
              return "Check error : Lc inconsistant with TLV structure";
            case 0x86:
              return "Check error : inconsistant parameters P1-P2";
            case 0x87:
              return "Check error : P3 inconsistant with P1-P2";
            case 0x88:
              return "Check error : referenced data not found";
            default:
              return "Check error : wrong parameters";
          }

        case 0x6B:
          return "Check error : reference P1,P2 incorrect";

        case 0x6C:
          return "Lc length incorrect, correct length :" + SW2.ToString();

        case 0x6D:
          return "Ins invalid or unsupported";

        case 0x6E:
          return "Cla insupported";

        case 0x6F:
          int rc = 0 - SW2;
          switch(rc)
          {
            /* Error codes taken from SpringProx API */
            case 0 :
              return "Undiagnosticed error";
            case -1 :
              return "No answer (no card / card is mute)";
            case -2 :
              return "Invalid CRC in card's response";
            case -3 :
              return "No frame received (NFC mode)";
            case -4 :
              return "Card: Authentication failed or access denied";
            case -5 :
              return "Invalid parity bit(s) in card's response";
            case -6 :
              return "NACK or status indicating error";
            case -7 :
              return "Too many anticollision loops";
            case -8 :
              return "Wrong LRC in card's serial number";
            case -9 :
              return "Card or block locked";
            case -10 :
              return "Card: Authentication must be performed first";
            case -11 :
              return "Wrong number of bits in card's answer";
            case -12 :
              return "Wrong number of bytes in card's answer";
            case -13 :
              return "Card: Counter is invalid";
            case -14 :
              return "Card: Transaction error";
            case -15 :
              return "Card: Write failed";
            case -16 :
              return "Card: Counter increase failed";
            case -17 :
              return "Card: Counter decrease failed";
            case -18 :
              return "Card: Read failed";
            case -19 :
              return "RC: FIFO overflow";
            case -20 :
              return "Polling mode pending";
            case -21 :
              return "Invalid framing in card's response";
            case -22 :
              return "Card: Access error (bad address or denied)";
            case -23 :
              return "RC: Unknown command";
            case -24 :
              return "A collision has occurred";
            case -25 :
              return "Command execution failed";
            case -26 :
              return "Hardware error";
            case -27 :
              return "RC: timeout";
            case -28 :
              return "More than one card found, but at least one does not support anticollision";
            case -29 :
              return "An external RF field has been detected";
            case -30 :
              return "Polling terminated (timeout or break)";
            case -31 :
              return "Bogus status in card's response";
            case -32 :
              return "Card: Vendor specific error";
            case -33 :
              return "Card: Command not supported";
            case -34 :
              return "Card: Format of command invalid";
            case -35 :
              return "Card: Option(s) of command invalid";
            case -36 :
              return "Card: other error";
            case -59 :
              return "Command not available in this mode";
            case -60 :
              return "Wrong parameter for the command";
            case -71 :
              return "No active card with this CID";
            case -75 :
              return "Length error in card's ATS";
            case -76 :
              return "Error in card's response to ATTRIB";
            case -77 :
              return "Format error in card's ATS";
            case -78 :
              return "Protocol error in card's response";
            case -87 :
              return "Format error in card's PPS response";
            case -88 :
              return "Other error in card's PPS response";
            case -93 :
              return "A card is already active with this CID";
            case -100 :
              return "Command not supported by the coupler";
            case -111 :
              return "Internal error in the coupler";
            case -112 :
              return "Internal buffer overflow";
            case -125 :
              return "Wrong data length for the command";
            case -128 :
              return "More time needed to process the command";              
            default :
              return "Undiagnosticed error " + rc;
          }         

        case 0x90:
          switch (SW2)
          {
            case 0x00:
              return "Success";
            case 0x01:
              return "Failed to write EEPROM";
            case 0x10:
              return "Wrong PIN (1st try)";
            case 0x20:
              return "Wrong PIN(2nd try)";
            case 0x40:
              return "Wrong PIN (3rd try)";
            case 0x80:
              return "Wrong PIN, card blocked";
            default:
              return "Unknown error";
          }

        case 0x92:
          switch (SW2)
          {
            case 0x00:
              return "Reference executed ok";
            case 0x02:
              return "Failed to write EEPROM";
            default:
              return "Unknow error";
          }

        case 0x94:
          return "No EF selected";

        case 0x98:
          switch (SW2)
          {
            case 0x02:
              return "Invalid PIN";
            case 0x04:
              return "Wrong PIN presentation";
            case 0x06:
              return "PIN cancelled";
            case 0x08:
              return "PIN inactivated";
            case 0x10:
              return "Security condition unsatisfied";
            case 0x20:
              return "PIN inactive";
            default:
              return "Unknown error";
          }

        default:
          return "Unknown error";
      }
    }
  }
#endregion

#region SCardReaderList  
  
/**c* SpringCardPCSC/SCardReaderList
 *
 * NAME
 *   SCardReaderList
 *
 * DESCRIPTION
 *   The SCardReaderList object is used to monitor a set of PC/SC readers (i.e. wait for card events)
 *
 * SYNOPSIS
 *   SCardReaderList( string[] reader_names );
 *
 **/

  public class SCardReaderList
  {    
    uint _last_error;
    uint _scope = SCARD.SCOPE_SYSTEM;
    string _groups = null;
    string[] _reader_names;
    bool _auto_update_list;
    StatusChangeCallback _status_change_callback = null;
    Thread _status_change_thread = null;
    volatile bool _status_change_running = false;

    public SCardReaderList(uint Scope, string Groups)
    {
      _scope = Scope;
      _groups = Groups;
      _reader_names = null;
      _auto_update_list = true;
    }

    public SCardReaderList()
    {
      _reader_names = null;
      _auto_update_list = true;
    }

    public SCardReaderList(string[] reader_names)
    {
      _reader_names = reader_names;
      _auto_update_list = false;
    }

     ~SCardReaderList()		
    {
      StopMonitor();
    }

/**t* SCardReaderList/StatusChangeCallback
 *
 * NAME
 *   SCardReaderList.StatusChangeCallback
 *
 * SYNOPSIS
 *   delegate void StatusChangeCallback(string ReaderName, uint ReaderState, CardBuffer CardAtr);
 *
 * DESCRIPTION
 *   Typedef for the callback that will be called by the background thread launched
 *   by SCardReaderList.StartMonitor(), everytime the status of one of the readers is changed.
 *
 * NOTES
 *   The callback is invoked in the context of a background thread. This implies that
 *   it is not allowed to access the GUI's components directly.
 *
 **/
    public delegate void StatusChangeCallback(string ReaderName,
                                              uint ReaderState,
                                              CardBuffer CardAtr);

/**m* SCardReaderList/StartMonitor
 *
 * NAME
 *   SCardReaderList.StartMonitor()
 *
 * SYNOPSIS
 *   SCardReaderList.StartMonitor(SCardReaderList.StatusChangeCallback callback);
 *
 * DESCRIPTION
 *   Create a background thread to monitor the reader associated to the object.
 *   Everytime the status of the reader is changed, the callback is invoked.
 *
 * SEE ALSO
 *   SCardReaderList.StatusChangeCallback
 *   SCardReaderList.StopMonitor()
 *
 **/
    public void StartMonitor(StatusChangeCallback callback)
    {
      StopMonitor();

      if (callback != null)
      {
        _status_change_callback = callback;
        _status_change_thread = new Thread(StatusChangeMonitor);
        _status_change_running = true;
        _status_change_thread.Start();
      }
    }

/**m* SCardReaderList/StopMonitor
 *
 * NAME
 *   SCardReaderList.StopMonitor()
 *
 * DESCRIPTION
 *   Stop the background thread previously launched by SCardReaderList.StartMonitor().
 *
 **/
    public void StopMonitor()
    {
      _status_change_callback = null;
      _status_change_running = false;

      if (_status_change_thread != null)
      {
        _status_change_thread.Abort();
        _status_change_thread.Join();
        _status_change_thread = null;
      }
    }

    private void StatusChangeMonitor()
    {    
      IntPtr hContext = IntPtr.Zero;

      _last_error = SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
      if (_last_error != SCARD.S_SUCCESS)
      {
        _status_change_callback(null, 0, null);
        return;
      }
      
      uint global_notification_state = SCARD.STATE_UNAWARE;
        
      while (_status_change_running)
      {
        /* Construct the list of readers we'll have to monitor */
        /* --------------------------------------------------- */
        
        bool global_notification_fired = false;
                
        SCARD.READERSTATE[] states;
        
        if (_auto_update_list)
        {        
          if (_reader_names != null)
            states = new SCARD.READERSTATE[_reader_names.Length + 1];
          else
            states = new SCARD.READERSTATE[1];
        } else
        {
          if (_reader_names == null)
          {
            SCARD.ReleaseContext(hContext);
            _status_change_callback(null, 0, null);
            return;
          }
          states = new SCARD.READERSTATE[_reader_names.Length];
        }
        
        for (int i=0; i<states.Length; i++)
        {
    	    states[i] = new SCARD.READERSTATE();
    	    if (_auto_update_list && (i == 0))
    	    {
    	      /* Magic string to be notified of reader arrival/removal */
    	      states[i].szReader = "\\\\?PNP?\\NOTIFICATION";
    	      states[i].dwCurrentState = global_notification_state;
    	    } else
    	    {
    	      /* Reader name */
    	      states[i].szReader = _reader_names[i-1];
    	      states[i].dwCurrentState = SCARD.STATE_UNAWARE;
    	    }
    	    states[i].dwEventState = 0;
    	    states[i].cbAtr = 0;
    	    states[i].rgbAtr = null;
    	    states[i].pvUserData = IntPtr.Zero;
        }
        
        /* Now wait for an event */
        /* --------------------- */
  
        while (_status_change_running && !global_notification_fired)
        {
          uint rc = SCARD.GetStatusChange(hContext, 250, states, (uint) states.Length);
  
          if (!_status_change_running)
            break;
    
          if (rc == SCARD.E_TIMEOUT)
            continue;
    
          if (rc != SCARD.S_SUCCESS)
          {
            _last_error = rc;
            /* Broadcast a message saying we have a problem! */
            for (int i=0; i<states.Length; i++)
              states[i].dwEventState = 0|SCARD.STATE_CHANGED;
          }
            
          for (int i=0; i<states.Length; i++)
          {
            if ((states[i].dwEventState & SCARD.STATE_CHANGED) != 0)
            {
    		      /* This reader has fired an event */
    		      /* ------------------------------ */

      		    if (_auto_update_list && (i == 0))
      		    {
      		      /* Not a reader but \\\\?PNP?\\NOTIFICATION */
      		      /* ---------------------------------------- */
  
      		      global_notification_fired = true;
      		      global_notification_state = states[0].dwEventState;
      		      
      		      /* Refresh the list of readers */          
                _reader_names = SCARD.GetReaderList(hContext, _groups);
  
                /* Notify the application that the list of readers has changed */
                _status_change_callback(null, states[0].dwEventState & ~SCARD.STATE_CHANGED, null);

      		    } else
      		    {
      		      /* This is a reader */
      		      /* ---------------- */
      		      
      		      states[i].dwCurrentState = states[i].dwEventState;
      		      if ((states[i].dwCurrentState & SCARD.STATE_IGNORE) != 0)
      		        states[i].dwCurrentState = SCARD.STATE_UNAVAILABLE;
      		      
                CardBuffer card_atr = null;
    
                /* Is there a card involved ? */
                if ((states[i].dwEventState & SCARD.STATE_PRESENT) != 0)
                  card_atr = new CardBuffer(states[i].rgbAtr, (int) states[i].cbAtr);  
                  
                _status_change_callback(states[i].szReader, states[i].dwEventState & ~SCARD.STATE_CHANGED, card_atr);
              }
            }
          }
        }
      }

      SCARD.ReleaseContext(hContext);
    }
    
/**f* SCardReaderList/Readers
 *
 * NAME
 *   SCardReaderList.Readers
 *
 * DESCRIPTION
 *   Provides the list of the monitored PC/SC readers
 *
 * SYNOPSIS
 *   string[] SCardReaderList.Readers
 *
 **/
    public string[] Readers
    {
      get
      {
        return _reader_names;
      }
    }

/**v* SCardReaderList/LastError
 *
 * NAME
 *   uint SCardReaderList.LastError
 * 
 * OUTPUT
 *   Returns the last error encountered by the object when working with SCARD functions.
 *   
 * SEE ALSO
 *   SCardReaderList.LastErrorAsString
 *
 **/
    public uint LastError
    {
      get
      {
        return _last_error;
      }
    }

/**v* SCardReaderList/LastErrorAsString
 *
 * NAME
 *   string SCardReaderList.LastErrorAsString
 * 
 * OUTPUT
 *   Returns the last error encountered by the object when working with SCARD functions.
 *   
 * SEE ALSO
 *   SCardReaderList.LastError
 *
 **/
    public string LastErrorAsString
    {
      get
      {
        return SCARD.ErrorToString(_last_error);
      }
    }

  }
#endregion
  
#region SCardReader class

/**c* SpringCardPCSC/SCardReader
 *
 * NAME
 *   SCardReader
 *
 * DESCRIPTION
 *   The SCardReader object is used to monitor a PC/SC reader (i.e. wait for card events)
 *
 * SYNOPSIS
 *   SCardReader( string reader_name );
 *
 **/

  public class SCardReader
  {
    uint _last_error;
    uint _scope = SCARD.SCOPE_SYSTEM;
    string _reader_name;
    uint _reader_state = SCARD.STATE_UNAWARE;
    CardBuffer _card_atr = null;
    StatusChangeCallback _status_change_callback = null;
    Thread _status_change_thread = null;
    volatile bool _status_change_running = false;

    public SCardReader(uint Scope, string ReaderName)
    {
      _scope = Scope;
      _reader_name = ReaderName;
    }

    public SCardReader(string ReaderName)
    {
      _reader_name = ReaderName;
    }

     ~SCardReader()		
    {
      StopMonitor();
    }
     
     public uint Scope
     {
       get
       {
         return _scope;
       }
     }

/**v* SCardReader/Name
 *
 * NAME
 *   SCardReader.Name
 *
 * SYNOPSIS
 *   string Name
 *
 * OUTPUT
 *   Return the name of the reader specified when instanciating the object.
 *
 **/

    public string Name
    {
      get
      {
        return _reader_name;
      }
    }

/**t* SCardReader/StatusChangeCallback
 *
 * NAME
 *   SCardReader.StatusChangeCallback
 *
 * SYNOPSIS
 *   delegate void StatusChangeCallback(uint ReaderState, CardBuffer CardAtr);
 *
 * DESCRIPTION
 *   Typedef for the callback that will be called by the background thread launched
 *   by SCardReader.StartMonitor(), everytime the status of the reader is changed.
 *
 * NOTES
 *   The callback is invoked in the context of a background thread. This implies that
 *   it is not allowed to access the GUI's components directly.
 *
 **/
    public delegate void StatusChangeCallback(uint ReaderState,
                                              CardBuffer CardAtr);

/**m* SCardReader/StartMonitor
 *
 * NAME
 *   SCardReader.StartMonitor()
 *
 * SYNOPSIS
 *   SCardReader.StartMonitor(SCardReader.StatusChangeCallback callback);
 *
 * DESCRIPTION
 *   Create a background thread to monitor the reader associated to the object.
 *   Everytime the status of the reader is changed, the callback is invoked.
 *
 * SEE ALSO
 *   SCardReader.StatusChangeCallback
 *   SCardReader.StopMonitor()
 *
 **/

    public void StartMonitor(StatusChangeCallback callback)
    {
      StopMonitor();

      if (callback != null)
      {
        _status_change_callback = callback;
        _status_change_thread = new Thread(StatusChangeMonitor);
        _status_change_running = true;
        _status_change_thread.Start();
      }
    }

/**m* SCardReader/StopMonitor
 *
 * NAME
 *   SCardReader.StopMonitor()
 *
 * DESCRIPTION
 *   Stop the background thread previously launched by SCardReader.StartMonitor().
 *
 **/

    public void StopMonitor()
    {
      _status_change_callback = null;
      _status_change_running = false;

      if (_status_change_thread != null)
      {
        _status_change_thread.Abort();
        _status_change_thread.Join();
        _status_change_thread = null;
      }
    }

    private void StatusChangeMonitor()
    {
      uint rc;

      IntPtr hContext = IntPtr.Zero;

      _reader_state = SCARD.STATE_UNAWARE;
      _card_atr = null;

      rc =
        SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
      if (rc != SCARD.S_SUCCESS)
        return;

      SCARD.READERSTATE[]states = new SCARD.READERSTATE[1];

      states[0] = new SCARD.READERSTATE();
      states[0].szReader = _reader_name;
      states[0].pvUserData = IntPtr.Zero;
      states[0].dwCurrentState = 0;
      states[0].dwEventState = 0;
      states[0].cbAtr = 0;
      states[0].rgbAtr = null;

      while (_status_change_running)
      {
        rc = SCARD.GetStatusChange(hContext, 250, states, 1);

        if (!_status_change_running)
          break;

        if (rc == SCARD.E_TIMEOUT)
          continue;

        if (rc != SCARD.S_SUCCESS)
        {
          _last_error = rc;

          SCARD.ReleaseContext(hContext);
          if (_status_change_callback != null)
            _status_change_callback(0, null);
          break;
        }

        if ((states[0].dwEventState & SCARD.STATE_CHANGED) != 0)
        {
          states[0].dwCurrentState = states[0].dwEventState;
          
          if (_status_change_callback != null)
          {
            CardBuffer card_atr = null;

            if ((states[0].dwEventState & SCARD.STATE_PRESENT) != 0)
              card_atr =
                new CardBuffer(states[0].rgbAtr, (int) states[0].cbAtr);

            _status_change_callback(states[0].dwEventState & ~SCARD.
                                    STATE_CHANGED, card_atr);
          }
        }
      }

      SCARD.ReleaseContext(hContext);
    }

    private void UpdateState()
    {
      uint rc;

      IntPtr hContext = IntPtr.Zero;

      _reader_state = SCARD.STATE_UNAWARE;
      _card_atr = null;

      rc =
        SCARD.EstablishContext(_scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
      if (rc != SCARD.S_SUCCESS)
      {
        _last_error = rc;
        return;
      }

      SCARD.READERSTATE[]states = new SCARD.READERSTATE[1];

      states[0] = new SCARD.READERSTATE();
      states[0].szReader = _reader_name;
      states[0].pvUserData = IntPtr.Zero;
      states[0].dwCurrentState = 0;
      states[0].dwEventState = 0;
      states[0].cbAtr = 0;
      states[0].rgbAtr = null;

      rc = SCARD.GetStatusChange(hContext, 0, states, 1);
      if (rc != SCARD.S_SUCCESS)
      {
        SCARD.ReleaseContext(hContext);
        return;
      }

      SCARD.ReleaseContext(hContext);

      _reader_state = states[0].dwEventState;

      if ((_reader_state & SCARD.STATE_PRESENT) != 0)
      {
        _card_atr = new CardBuffer(states[0].rgbAtr, (int) states[0].cbAtr);
      }
    }

/**v* SCardReader/Status
 *
 * NAME
 *   SCardReader.Status
 * 
 * SYNOPSIS
 *   uint Status
 *
 * OUTPUT
 *   Returns the current status of the reader.
 *   
 * SEE ALSO
 *   SCardReader.CardPresent
 *   SCardReader.StatusAsString
 *
 **/

    public uint Status
    {
      get
      {
        UpdateState();
        return _reader_state;
      }
    }

/**v* SCardReader/StatusAsString
 *
 * NAME
 *   SCardReader.StatusAsString
 * 
 * SYNOPSIS
 *   string StatusAsString
 *
 * OUTPUT
 *   Returns the current status of the reader, using SCARD.ReaderStatusToString as formatter.
 *   
 * SEE ALSO
 *   SCardReader.Status
 *
 **/

    public string StatusAsString
    {
      get
      {
        UpdateState();
        return SCARD.ReaderStatusToString(_reader_state);
      }
    }

/**v* SCardReader/CardPresent
 *
 * NAME
 *   SCardReader.CardPresent
 * 
 * SYNOPSIS
 *   bool CardPresent
 *
 * OUTPUT
 *   Returns true if a card is present in the reader.
 *   Returns false if there's no smartcard in the reader.
 *   
 * SEE ALSO
 *   SCardReader.CardAtr
 *   SCardReader.CardAvailable
 *   SCardReader.Status
 *
 **/

    public bool CardPresent
    {
      get
      {
        UpdateState();
        if ((_reader_state & SCARD.STATE_PRESENT) != 0)
          return true;
        return false;
      }
    }

/**v* SCardReader/CardAvailable
 *
 * NAME
 *   SCardReader.CardAvailable
 *
 * SYNOPSIS
 *   bool CardAvailable
 *   
 * OUTPUT
 *   Returns true if a card is available in the reader.
 *   Returns false if there's no smartcard in the reader, or if it is already used by another process/thread.
 * 
 * SEE ALSO
 *   SCardReader.CardAtr
 *   SCardReader.CardPresent 
 *   SCardReader.Status
 *
 **/

    public bool CardAvailable
    {
      get
      {
        UpdateState();
        if (((_reader_state & SCARD.STATE_PRESENT) != 0)
            && ((_reader_state & SCARD.STATE_MUTE) == 0)
            && ((_reader_state & SCARD.STATE_INUSE) == 0))
          return true;
        return false;
      }
    }

/**v* SCardReader/CardAtr
 *
 * NAME
 *   SCardReader.CardAtr
 *
 * SYNOPSIS
 *   CardBuffer CardAtr
 *   
 * OUTPUT
 *   If a smartcard is present in the reader (SCardReader.CardPresent == true), returns the ATR of the card.
 *   Returns null overwise.
 * 
 * SEE ALSO
 *   SCardReader.CardPresent 
 *   SCardReader.Status
 *
 **/

    public CardBuffer CardAtr
    {
      get
      {
        UpdateState();
        return _card_atr;
      }
    }

/**v* SCardReader/LastError
 *
 * NAME
 *   uint SCardReader.LastError
 * 
 * OUTPUT
 *   Returns the last error encountered by the object when working with SCARD functions.
 *   
 * SEE ALSO
 *   SCardReader.LastErrorAsString
 *
 **/
    public uint LastError
    {
      get
      {
        return _last_error;
      }
    }

/**v* SCardReader/LastErrorAsString
 *
 * NAME
 *   string SCardReader.LastErrorAsString
 * 
 * OUTPUT
 *   Returns the last error encountered by the object when working with SCARD functions.
 *   
 * SEE ALSO
 *   SCardReader.LastError
 *
 **/
    public string LastErrorAsString
    {
      get
      {
        return SCARD.ErrorToString(_last_error);
      }
    }

  }
#endregion

#region SCardChannel class

/**c* SpringCardPCSC/SCardChannel
 *
 * NAME
 *   SCardChannel
 * 
 * DESCRIPTION
 *   The SCardChannel object provides the actual connection to the smartcard through the PC/SC reader
 *
 * SYNOPSIS
 *   SCardChannel( string reader_name );
 *   SCardChannel( SCardReader reader );
 *
 **/

  public class SCardChannel
  {
    private string _reader_name;
    private uint _reader_state;
    private uint _active_protocol;
    private uint _want_protocols = SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1;
    private uint _share_mode = SCARD.SHARE_SHARED;
    private uint _last_error;
    private CAPDU _capdu;
    private RAPDU _rapdu;
    private CardBuffer _cctrl;
    private CardBuffer _rctrl;
    private CardBuffer _card_atr;
    private IntPtr _hContext = IntPtr.Zero;
    private IntPtr _hCard = IntPtr.Zero;
    TransmitDoneCallback _transmit_done_callback = null;
    Thread _transmit_thread = null;

    public delegate void TransmitDoneCallback(RAPDU rapdu);
    
    private void Instanciate(uint Scope, string ReaderName)
    {
      uint rc;

      rc = SCARD.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, ref _hContext);
      if (rc != SCARD.S_SUCCESS)
      {
        _hContext = IntPtr.Zero;
        _last_error = rc;
      }

      _reader_name = ReaderName;
    }

    public SCardChannel(uint Scope, string ReaderName)
    {
      Instanciate(Scope, ReaderName);
    }

    public SCardChannel(string ReaderName)
    {
      Instanciate(SCARD.SCOPE_SYSTEM, ReaderName);
    }
    
    public SCardChannel(SCardReader Reader)
    {
      Instanciate(Reader.Scope, Reader.Name);
    }

    ~SCardChannel()
    {
      if (Connected)
        DisconnectReset();

      if (_hContext != IntPtr.Zero)
        SCARD.ReleaseContext(_hContext);
    }

    public IntPtr hContext
    {
      get
      {
        return _hContext;
      }
    }

    public IntPtr hCard
    {
      get
      {
        return _hCard;
      }
    }
    
    public string ReaderName
    {
      get
      {
        return _reader_name;
      }
    }

    private void UpdateState()
    {
      uint rc;

      _reader_state = SCARD.STATE_UNAWARE;
      _card_atr = null;

      if (Connected)
      {
        byte[]atr_buffer = new byte[36];
        uint atr_length = 36;

        uint dummy = 0;

        rc =
          SCARD.Status(_hCard, IntPtr.Zero, ref dummy,
                       ref _reader_state, ref _active_protocol, atr_buffer,
                       ref atr_length);
        if (rc != SCARD.S_SUCCESS)
        {
          _last_error = rc;
          return;
        }

        _card_atr = new CardBuffer(atr_buffer, (int) atr_length);


      } else
      {
        SCARD.READERSTATE[]states = new SCARD.READERSTATE[1];

        states[0] = new SCARD.READERSTATE();
        states[0].szReader = _reader_name;
        states[0].pvUserData = IntPtr.Zero;
        states[0].dwCurrentState = 0;
        states[0].dwEventState = 0;
        states[0].cbAtr = 0;
        states[0].rgbAtr = null;

        rc = SCARD.GetStatusChange(_hContext, 0, states, 1);
        if (rc != SCARD.S_SUCCESS)
        {
          _last_error = rc;
          return;
        }

        _reader_state = states[0].dwEventState;

        if ((_reader_state & SCARD.STATE_PRESENT) != 0)
        {
          _card_atr = new CardBuffer(states[0].rgbAtr, (int) states[0].cbAtr);
        }
      }
    }

/**v* SCardChannel/CardPresent
 *
 * NAME
 *   SCardChannel.CardPresent
 *
 * SYNOPSIS
 *   bool CardPresent
 *   
 * OUTPUT
 *   Returns true if a card is present in the reader associated to the SCardChannel object.
 *   Returns false if there's no smartcard in the reader.
 * 
 * SEE ALSO
 *   SCardChannel.CardAvailable
 *   SCardChannel.CardAtr
 *
 **/

    public bool CardPresent
    {
      get
      {
        UpdateState();

        if ((_reader_state & SCARD.STATE_PRESENT) != 0)
          return true;

        return false;
      }
    }

/**v* SCardChannel/CardAvailable
 *
 * NAME
 *   SCardChannel.CardAvailable
 *
 * SYNOPSIS
 *   bool CardAvailable
 *   
 * OUTPUT
 *   Returns true if a card is available in the reader associated to the SCardChannel object.
 *   Returns false if there's no smartcard in the reader, or if it is already used by another process/thread.
 * 
 * SEE ALSO
 *   SCardChannel.CardPresent
 *   SCardChannel.CardAtr
 *
 **/

    public bool CardAvailable
    {
      get
      {
        UpdateState();

        if (((_reader_state & SCARD.STATE_PRESENT) != 0)
            && ((_reader_state & SCARD.STATE_MUTE) == 0)
            && ((_reader_state & SCARD.STATE_INUSE) == 0))
          return true;

        return false;
      }
    }

/**v* SCardChannel/CardAtr
 *
 * NAME
 *   SCardChannel.CardAtr
 *
 * SYNOPSIS
 *   CardBuffer CardAtr
 *   
 * OUTPUT
 *   Returns the ATR of the smartcard in the reader, or null if no smartcard is present.
 * 
 * SEE ALSO
 *   SCardChannel.CardPresent
 *
 **/

    public CardBuffer CardAtr
    {
      get
      {
        UpdateState();
        return _card_atr;
      }
    }

/**v* SCardChannel/Connected
 *
 * NAME
 *   SCardChannel.Connected
 *
 * SYNOPSIS
 *   bool Connected
 *   
 * OUTPUT
 *   Returns true if the SCardChannel object is actually connected to a smartcard.
 *   Returns false if not.
 *
 **/

    public bool Connected
    {
      get
      {
        if (_hCard != IntPtr.Zero)
          return true;

        return false;
      }
    }

/**v* SCardChannel/Protocol
 *
 * NAME
 *   SCardChannel.Protocol
 *
 * SYNOPSIS
 *   uint Protocol
 * 
 * INPUTS
 *   Before the smartcard has been 
 * 
 * 
 * , set Protocol to specify the communication protocol(s) to be used
 *   by Connect(). Allowed values are SCARD.PROTOCOL_T0, SCARD.PROTOCOL_T1 or SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1.
 *   
 * OUTPUT
 *   Once the smartcard has been connected (Connected == true), Protocol is the current communication protocol.
 *   Possible values are SCARD.PROTOCOL_T0 or SCARD.PROTOCOL_T1.
 * 
 * SEE ALSO
 *   SCardChannel.ProtocolAsString
 *
 **/
    public uint Protocol
    {
      get
      {
        return _active_protocol;
      }
      set
      {
        _want_protocols = value;
      }
    }


/**v* SCardChannel/ProtocolAsString
 *
 * NAME
 *   SCardChannel.ProtocolAsString
 *
 * SYNOPSIS
 *   string ProtocolAsString
 * 
 * INPUTS
 *   Before the smartcard has been connected, set ProtocolAsString to specify the communication protocol(s) to be used
 *   by Connect(). Allowed values are "T=0", "T=1" or "*" (or "T=0|T=1").
 *   
 * OUTPUT
 *   Once the smartcard has been connected (Connected == true), ProtocolAsString is the current communication protocol.
 *   Possible values are "T=0" or "T=1".
 *   
 * SEE ALSO
 *   SCardChannel.Protocol
 *
 **/
    public string ProtocolAsString
    {
      get
      {
        return SCARD.CardProtocolToString(_active_protocol);
      }
      set
      {
        value = value.ToUpper();

        if (value.Equals("T=0"))
        {
          _want_protocols = SCARD.PROTOCOL_T0;
        } else
        if (value.Equals("T=1"))
        {
          _want_protocols = SCARD.PROTOCOL_T1;
        } else
        if (value.Equals("*") || value.Equals("AUTO") || value.Equals("T=0|T=1"))
        {
          _want_protocols = SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1;
        } else
        if (value.Equals("RAW"))
        {
          _want_protocols = SCARD.PROTOCOL_RAW;
        } else
        if (value.Equals("") || value.Equals("NONE") || value.Equals("DIRECT"))
        {
          _want_protocols = SCARD.PROTOCOL_NONE;
          _share_mode = SCARD.SHARE_DIRECT;
        }
      }
    }

/**v* SCardChannel/ShareMode
 *
 * NAME
 *   SCardChannel.ShareMode
 *
 * SYNOPSIS
 *   uint ShareMode
 * 
 * INPUTS
 *   Before the smartcard has been connected, set ShareMode to specify the sharing mode to be used
 *   by Connect(). Allowed values are SCARD.SHARE_EXCLUSIVE, SCARD.SHARE_SHARED or SCARD.SHARE_DIRECT.
 *   
 * SEE ALSO
 *   SCardChannel.ShareModeAsString
 *
 **/
    public uint ShareMode
    {
      get
      {
        return _share_mode;
      }
      set
      {
        _share_mode = value;
      }
    }

/**v* SCardChannel/ShareModeAsString
 *
 * NAME
 *   SCardChannel.ShareModeAsString
 *
 * SYNOPSIS
 *   string ShareModeAsString
 * 
 * INPUTS
 *   Before the smartcard has been connected, set ShareModeAsString to specify the sharing mode to be used
 *   by Connect(). Allowed values are "EXCLUSIVE", "SHARED" or "DIRECT".
 *   
 * SEE ALSO
 *   SCardChannel.ShareMode
 *
 **/
    public string ShareModeAsString
    {
      get
      {
        return SCARD.CardShareModeToString(_share_mode);
      }
      set
      {
        value = value.ToUpper();

        if (value.Equals("EXCLUSIVE"))
        {
          _share_mode = SCARD.SHARE_EXCLUSIVE;
        } else
        if (value.Equals("SHARED"))
        {
          _share_mode = SCARD.SHARE_SHARED;
        } else
        if (value.Equals("DIRECT"))
        {
          _want_protocols = SCARD.PROTOCOL_NONE;
          _share_mode = SCARD.SHARE_DIRECT;
        }
      }
    }

/**m* SCardChannel/Connect
 *
 * NAME
 *   SCardChannel.Connect()
 *
 * SYNOPSIS
 *   bool Connect()
 * 
 * DESCRIPTION
 *   Open the connection channel to the smartcard (according to the specified Protocol, default is either T=0 or T=1)
 *   
 * OUTPUT
 *   Returns true if the connection has been successfully established.
 *   Returns false if not. See LastError for details.
 * 
 * SEE ALSO
 *   SCardChannel.CardPresent
 *   SCardChannel.CardAtr
 *   SCardChannel.Protocol
 *   SCardChannel.Transmit
 *
 **/

    public bool Connect()
    {
      uint rc;

      if (Connected)
        return false;

      rc =
        SCARD.Connect(_hContext, _reader_name, _share_mode, _want_protocols,
                      ref _hCard, ref _active_protocol);
      if (rc != SCARD.S_SUCCESS)
      {
        _hCard = IntPtr.Zero;
        _last_error = rc;
        return false;
      }

      UpdateState();
      return true;
    }

/**m* SCardChannel/Disconnect
 *
 * NAME
 *   SCardChannel.Disconnect()
 *
 * SYNOPSIS
 *   bool Disconnect()
 *   bool Disconnect(uint disposition)
 * 
 * DESCRIPTION
 *   Close the connection channel
 *   
 * INPUTS
 *   The disposition parameter must take one of the following values:
 *   - SCARD.EJECT_CARD
 *   - SCARD.UNPOWER_CARD
 *   - SCARD.RESET_CARD
 *   - SCARD.LEAVE_CARD
 *   If this parameter is omitted, it defaults to SCARD.RESET_CARD
 * 
 * SEE ALSO
 *   SCardChannel.Connect
 *
 **/

    public bool Disconnect(uint disposition)
    {
      uint rc;

      rc = SCARD.Disconnect(_hCard, disposition);
      if (rc != SCARD.S_SUCCESS)
        _last_error = rc;

      _hCard = IntPtr.Zero;
      
      if (rc != SCARD.S_SUCCESS)
        return false;
      
      return true;
    }

    public bool Disconnect()
    {
    	return DisconnectReset();
    }

/**m* SCardChannel/DisconnectEject
 *
 * NAME
 *   SCardChannel.DisconnectEject()
 *
 * SYNOPSIS
 *   bool DisconnectEject()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Disconnect(SCARD.EJECT_CARD)
 *
 **/

    public bool DisconnectEject()
    {
      return Disconnect(SCARD.EJECT_CARD);
    }

/**m* SCardChannel/DisconnectUnpower
 *
 * NAME
 *   SCardChannel.DisconnectUnpower()
 *
 * SYNOPSIS
 *   bool DisconnectUnpower()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Disconnect(SCARD.UNPOWER_CARD)
 *
 **/

    public bool DisconnectUnpower()
    {
      return Disconnect(SCARD.UNPOWER_CARD);
    }

/**m* SCardChannel/DisconnectReset
 *
 * NAME
 *   SCardChannel.DisconnectReset()
 *
 * SYNOPSIS
 *   bool DisconnectReset()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Disconnect(SCARD.RESET_CARD)
 *
 **/

    public bool DisconnectReset()
    {
      return Disconnect(SCARD.RESET_CARD);
    }

/**m* SCardChannel/DisconnectLeave
 *
 * NAME
 *   SCardChannel.DisconnectLeave()
 *
 * SYNOPSIS
 *   void DisconnectLeave()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Disconnect(SCARD.LEAVE_CARD)
 *
 **/

    public bool DisconnectLeave()
    {
      return Disconnect(SCARD.LEAVE_CARD);
    }

/**m* SCardChannel/Reconnect
 *
 * NAME
 *   SCardChannel.Reconnect()
 *
 * SYNOPSIS
 *   bool Reconnect()
 *   bool Reconnect(uint disposition)
 * 
 * DESCRIPTION
 *   Re-open the connection channel to the smartcard
 *   
 * INPUTS
 *   The disposition parameter must take one of the following values:
 *   - SCARD.EJECT_CARD
 *   - SCARD.UNPOWER_CARD
 *   - SCARD.RESET_CARD
 *   - SCARD.LEAVE_CARD
 *   If this parameter is omitted, it defaults to SCARD.RESET_CARD
 *   
 * OUTPUT
 *   Returns true if the connection has been successfully re-established.
 *   Returns false if not. See LastError for details.
 * 
 * SEE ALSO
 *   SCardChannel.Connect
 *   SCardChannel.Disconnect
 *
 **/

    public bool Reconnect(uint disposition)
    {
        uint rc;

        if (!Connected)
            return false;

        rc =
          SCARD.Reconnect(_hCard, _share_mode, _want_protocols, disposition, ref _active_protocol);
        if (rc != SCARD.S_SUCCESS)
        {
            _hCard = IntPtr.Zero;
            _last_error = rc;
            return false;
        }

        UpdateState();
        return true;
    }

    public void Reconnect()
    {
        ReconnectReset();
    }

/**m* SCardChannel/ReconnectEject
 *
 * NAME
 *   SCardChannel.ReconnectEject()
 *
 * SYNOPSIS
 *   void ReconnectEject()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Reconnect(SCARD.EJECT_CARD)
 *
 **/

    public void ReconnectEject()
    {
      Reconnect(SCARD.EJECT_CARD);
    }

/**m* SCardChannel/ReconnectUnpower
 *
 * NAME
 *   SCardChannel.ReconnectUnpower()
 *
 * SYNOPSIS
 *   void ReconnectUnpower()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Reconnect(SCARD.UNPOWER_CARD)
 *
 **/

    public void ReconnectUnpower()
    {
      Reconnect(SCARD.UNPOWER_CARD);
    }

/**m* SCardChannel/ReconnectReset
 *
 * NAME
 *   SCardChannel.ReconnectReset()
 *
 * SYNOPSIS
 *   void ReconnectReset()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Disconnect(SCARD.RESET_CARD)
 *
 **/

    public void ReconnectReset()
    {
      Reconnect(SCARD.RESET_CARD);
    }

/**m* SCardChannel/ReconnectLeave
 *
 * NAME
 *   SCardChannel.ReconnectLeave()
 *
 * SYNOPSIS
 *   void ReconnectLeave()
 * 
 * DESCRIPTION
 *   Same as SCardChannel.Reconnect(SCARD.LEAVE_CARD)
 *
 **/

    public void ReconnectLeave()
    {
      Reconnect(SCARD.LEAVE_CARD);
    }
    
/**m* SCardChannel/Transmit
 *
 * NAME
 *   SCardChannel.Transmit()
 *
 * SYNOPSIS
 *   bool  Transmit()
 *   RAPDU Transmit(CAPDU capdu)
 *   bool  Transmit(CAPDU capdu, ref RAPDU rapdu)
 *   void  Transmit(CAPDU capdu, TransmitDoneCallback callback)
 * 
 * DESCRIPTION
 *   Sends a command APDU (CAPDU) to the connected card, and retrieves its response APDU (RAPDU)
 *
 * SOURCE
 *
 *   SCardChannel card = new SCardChannel( ... reader ... );
 *   if (!card.Connect( SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1 ))
 *   {
 *     // handle error
 *   }
 *
 *
 *   // Example 1
 *   // ---------
 *
 *   card.Command = new CAPDU("00 A4 00 00 02 3F 00");
 *   if (!card.Transmit())
 *   {
 *     // handle error
 *   } 
 *   MessageBox.Show("Card answered: " + card.Response.AsString(" "));
 *
 *
 *   // Example 2
 *   // ---------
 *
 *   RAPDU response = card.Transmit(new CAPDU("00 A4 00 00 02 3F 00")))
 *   if (response == null)
 *   {
 *     // handle error
 *   }
 *   MessageBox.Show("Card answered: " + response.AsString(" "));
 *
 *
 *   // Example 3
 *   // ---------
 *
 *   CAPDU command  = new CAPDU("00 A4 00 00 02 3F 00");
 *   RAPDU response = new RAPDU();
 *   if (!card.Transmit(command, ref response))
 *   {
 *     // handle error
 *   } 
 *   MessageBox.Show("Card answered: " + response.AsString(" "));
 *
 *
 *   // Example 4
 *   // ---------
 *
 *   // In this example the Transmit is performed by a background thread
 *   // We supply a delegate so the main class (window/form) will be notified
 *   // when Transmit will return
 *
 *   delegate void OnTransmitDoneInvoker(RAPDU response);
 *
 *   void OnTransmitDone(RAPDU response)
 *   {
 *     // Ensure we're back in the context of the main thread (application's message pump)
 *     if (this.InvokeRequired)
 *     {
 *       this.BeginInvoke(new OnTransmitDoneInvoker(OnTransmitDone), response);
 *       return;
 *     }
 *      
 *     if (response == null)
 *     {
 *       // handle error
 *     }
 *
 *     MessageBox.Show("Card answered: " + response.AsString(" "));
 *   }
 *
 *  card.Transmit(new CAPDU("00 A4 00 00 02 3F 00"), new SCardChannel.TransmitDoneCallback(OnTransmitDone));
 *
 * 
 * SEE ALSO
 *   SCardChannel.Connect
 *   SCardChannel.Transmit
 *   SCardChannel.Command
 *   SCardChannel.Response
 *
 **/
#region Transmit
    public bool Transmit()
    {
      byte[]rsp_buffer = new byte[258];
      uint rsp_length = 258;
      uint rc;
      IntPtr SendPci = IntPtr.Zero;
      
      switch (_active_protocol)
      {
        case SCARD.PROTOCOL_T0 :
          SendPci = SCARD.PCI_T0();
          break;
        case SCARD.PROTOCOL_T1 :
          SendPci = SCARD.PCI_T1();
          break;
        case SCARD.PROTOCOL_RAW :
          SendPci = SCARD.PCI_RAW();
          break;
        default :
          break;
      }

      _rapdu = null;

      rc = SCARD.Transmit(_hCard,
                          SendPci,
                          _capdu.GetBytes(),
                          (uint) _capdu.Length,
                          IntPtr.Zero, /* RecvPci is likely to remain NULL */
                          rsp_buffer,
                          ref rsp_length);

      if (rc != SCARD.S_SUCCESS)
      {
        _last_error = rc;
        return false;
      }

      _rapdu = new RAPDU(rsp_buffer, (int) rsp_length);
      return true;
    }

    public bool Transmit(CAPDU capdu, ref RAPDU rapdu)
    {
      _capdu = capdu;

      if (!Transmit())
        return false;

      rapdu = _rapdu;
      return true;
    }

    public RAPDU Transmit(CAPDU capdu)
    {
      _capdu = capdu;

      if (!Transmit())
        return null;

      return _rapdu;
    }

    public void Transmit(CAPDU capdu, TransmitDoneCallback callback)
    {
      if (_transmit_thread != null)
        _transmit_thread = null;

      _capdu = capdu;

      if (callback != null)
      {
        _transmit_done_callback = callback;
        _transmit_thread = new Thread(TransmitFunction);
        _transmit_thread.Start();
      }
    }

    private void TransmitFunction()
    {
      if (Transmit())
      {
        if (_transmit_done_callback != null)
          _transmit_done_callback(_rapdu);
      } else
      {
        if (_transmit_done_callback != null)
          _transmit_done_callback(null);
      }
    }
    

/**v* SCardChannel/Command
 *
 * NAME
 *   SCardChannel.Command
 *
 * SYNOPSIS
 *   CAPDU Command
 * 
 * DESCRIPTION
 *   C-APDU to be sent to the card through SCardChannel.Transmit
 *
 **/
    public CAPDU Command
    {
      get
      {
        return _capdu;
      }
      set
      {
        _capdu = value;
      }
    }

/**v* SCardChannel/Response
 *
 * NAME
 *   SCardChannel.Response
 *
 * SYNOPSIS
 *   RAPDU Response
 * 
 * DESCRIPTION
 *   R-APDU returned by the card after a succesfull call to SCardChannel.Transmit
 *
 **/
    public RAPDU Response
    {
      get
      {
        return _rapdu;
      }
    }
#endregion

#region Control
    public byte[] Control(byte[] cctrl)
    {
      byte[] rctrl = new byte[280];
      uint rl = 0;
      uint rc;

      rc = SCARD.Control(_hCard,
                         SCARD.IOCTL_CSB6_PCSC_ESCAPE,
                         cctrl,
                         (uint) cctrl.Length,
                         rctrl,
                         280,
                         ref rl);

      if (rc == 1)
      {
        rc = SCARD.Control(_hCard,
                           SCARD.IOCTL_MS_CCID_ESCAPE,
                           cctrl,
                           (uint) cctrl.Length,
                           rctrl,
                           280,
                           ref rl);

      }
      
      if (rc != SCARD.S_SUCCESS)
      {
        _last_error = rc;
        rctrl = null;
        return null;
      }
      
      byte[] r = new byte[rl];
      for (int i=0; i<rl; i++)
        r[i] = rctrl[i];

      return r;
    }

    public bool Control()
    {
      byte[] r = Control(_cctrl.GetBytes());
      
      if (r == null)
        return false;
      
      _rctrl = null;
      
      if (r.Length > 0)
        _rctrl = new CardBuffer(r);

      return true;
    }
    
    public bool Control(CardBuffer cctrl, ref CardBuffer rctrl)
    {
      _cctrl = cctrl;

      if (!Control())
        return false;
      
      rctrl = _rctrl;
      return true;
    }

    public CardBuffer Control(CardBuffer cctrl)
    {
      _cctrl = cctrl;

      if (!Control())
        return null;

      return _rctrl;
    }

    public bool Leds(byte red, byte green, byte blue)
    {
      byte[]buffer = new byte[5];

      buffer[0] = 0x58;
      buffer[1] = 0x1E;
      buffer[2] = red;
      buffer[3] = green;
      buffer[4] = blue;

      if (Control(new CardBuffer(buffer)) != null)
        return true;

      return false;
    }
    
    public bool LedsDefault()
    {
      byte[]buffer = new byte[2];

      buffer[0] = 0x58;
      buffer[1] = 0x1E;

      if (Control(new CardBuffer(buffer)) != null)
        return true;

      return false;
    }

    public bool Buzzer(ushort duration_ms)
    {
      byte[]buffer = new byte[4];

      buffer[0] = 0x58;
      buffer[1] = 0x1C;
      buffer[2] = (byte) (duration_ms / 0x0100);
      buffer[3] = (byte) (duration_ms % 0x0100);

      if (Control(new CardBuffer(buffer)) != null)
        return true;

      return false;
    }
    
    public bool BuzzerDefault()
    {
      byte[]buffer = new byte[2];

      buffer[0] = 0x58;
      buffer[1] = 0x1C;

      if (Control(new CardBuffer(buffer)) != null)
        return true;

      return false;
    }
#endregion


/**v* SCardChannel/LastError
 *
 * NAME
 *   SCardChannel.LastError
 *
 * SYNOPSIS
 *   uint LastError
 * 
 * OUTPUT
 *   Returns the last error encountered by the object when working with SCARD functions.
 *   
 * SEE ALSO
 *   SCardChannel.LastErrorAsString
 *
 **/
    public uint LastError
    {
      get
      {
        return _last_error;
      }
    }

/**v* SCardChannel/LastErrorAsString
 *
 * NAME
 *   SCardChannel.LastErrorAsString
 *
 * SYNOPSIS
 *   string LastErrorAsString
 * 
 * OUTPUT
 *   Returns the last error encountered by the object when working with SCARD functions.
 *   
 * SEE ALSO
 *   SCardChannel.LastError
 *
 **/
    public string LastErrorAsString
    {
      get
      {
        return SCARD.ErrorToString(_last_error);
      }
    }

  }
#endregion

#region CardBuffer class

/**c* SpringCardPCSC/CardBuffer
 *
 * NAME
 *   CardBuffer
 * 
 * DESCRIPTION
 *   The CardBuffer provides convenient access to byte-arrays
 * 
 * DERIVED BY
 *   CAPDU
 *   RAPDU
 *
 **/

  public class CardBuffer
  {
    protected byte[] _bytes = null;

    private bool isb(char c)
    {
      bool r = false;

      if ((c >= '0') && (c <= '9'))
      {
        r = true;
      } else if ((c >= 'A') && (c <= 'F'))
      {
        r = true;
      } else if ((c >= 'a') && (c <= 'f'))
      {
        r = true;
      }

      return r;
    }

    private byte htob(char c)
    {
      int r = 0;

      if ((c >= '0') && (c <= '9'))
      {
        r = c - '0';
      } else if ((c >= 'A') && (c <= 'F'))
      {
        r = c - 'A';
        r += 10;
      } else if ((c >= 'a') && (c <= 'f'))
      {
        r = c - 'a';
        r += 10;
      }

      return (byte) r;
    }
    
    public CardBuffer()
    {

    }

    public CardBuffer(byte b)
    {
      _bytes = new byte[1];
      _bytes[0] = b;
    }

    public CardBuffer(ushort w)
    {
      _bytes = new byte[2];
      _bytes[0] = (byte) (w / 0x0100);
      _bytes[1] = (byte) (w % 0x0100);
    }

    public CardBuffer(byte[]bytes)
    {
      _bytes = bytes;
    }

    public CardBuffer(byte[]bytes, long length)
    {
      SetBytes(bytes, length);
    }

    public CardBuffer(byte[]bytes, long offset, long length)
    {
      SetBytes(bytes, offset, length);
    }

    public CardBuffer(string str)
    {
      SetString(str);
    }
    
    public byte GetByte(long offset)
    {
      if (_bytes == null)
        return 0;
      
      if (offset >= _bytes.Length)
        offset = 0;

      return _bytes[offset];
    }

    public byte[] GetBytes()
    {
      return _bytes;
    }

    public byte[] GetBytes(long length)
    {
      if (_bytes == null)
        return null;
      
      if (length < 0)
        length = _bytes.Length - length;
      
      if (length > _bytes.Length)
        length = _bytes.Length;
      
      byte[] r = new byte[length];
      for (long i=0; i<length; i++)
        r[i] = _bytes[i];
      
      return r;
    }

    public byte[] GetBytes(long offset, long length)
    {
      if (_bytes == null)
        return null;
      
      if (offset < 0)
        offset = _bytes.Length - offset;
      
      if (length < 0)
        length = _bytes.Length - length;

      if (offset >= _bytes.Length)
        return null;

      if (length > (_bytes.Length - offset))
        length = _bytes.Length - offset;
      
      byte[] r = new byte[length];
      for (long i=0; i<length; i++)
        r[i] = _bytes[offset+i];
      
      return r;
    }
    
    public char[] GetChars(long offset, long length)
    {
      byte[] b = GetBytes(offset, length);
      
      if (b == null) return null;
      
      char[] c = new char[b.Length];
      for (long i=0; i<b.Length; i++)
        c[i] = (char) b[i];
      
      return c;      
    }

    public void SetBytes(byte[]bytes)
    {
      _bytes = bytes;
    }

    public void SetBytes(byte[]bytes, long length)
    {
      _bytes = new byte[length];

      long i;

      for (i = 0; i < length; i++)
        _bytes[i] = bytes[i];
    }

    public void SetBytes(byte[]bytes, long offset, long length)
    {
      _bytes = new byte[length];

      long i;

      for (i = 0; i < length; i++)
        _bytes[i] = bytes[offset + i];
    }

    public void SetString(string str)
    {
      string s = "";
      int i, l;

      l = str.Length;
      for (i = 0; i < l; i++)
      {
        char c = str[i];

        if (isb(c))
          s = s + c;
      }

      l = s.Length;
      _bytes = new Byte[l / 2];

      for (i = 0; i < l; i += 2)
      {
        _bytes[i / 2] = htob(s[i]);
        _bytes[i / 2] *= 0x10;
        _bytes[i / 2] += htob(s[i + 1]);
      }
    }

    public string AsString(string separator)
    {
      string s = "";
      long i;

      if (_bytes != null)
      {
        for (i = 0; i < _bytes.Length; i++)
        {
          if (i > 0)
            s = s + separator;
          s = s + String.Format("{0:X02}", _bytes[i]);
        }
      }

      return s;
    }

    public string AsString()
    {
      return AsString("");
    }

    protected byte[] Bytes
    {
      get
      {
        return _bytes;
      }
      set
      {
        _bytes = value;
      }
    }

    public int Length
    {
      get
      {
        if (_bytes == null)
          return 0;
        return _bytes.Length;
      }
    }
  }
#endregion

#region CAPDU class

/**c* SpringCardPCSC/CAPDU
 *
 * NAME
 *   CAPDU
 * 
 * DESCRIPTION
 *   The CAPDU object is used to format and send COMMAND APDUs (according to ISO 7816-4) to the smartcard
 * 
 * DERIVED FROM
 *   CardBuffer
 *
 **/

  public class CAPDU:CardBuffer
  {
    public CAPDU()
    {

    }

    public CAPDU(byte[]bytes)
    {
      _bytes = bytes;
    }

    public CAPDU(byte CLA, byte INS, byte P1, byte P2)
    {
      _bytes = new byte[4];
      _bytes[0] = CLA;
      _bytes[1] = INS;
      _bytes[2] = P1;
      _bytes[3] = P2;
    }

    public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte P3)
    {
      _bytes = new byte[5];
      _bytes[0] = CLA;
      _bytes[1] = INS;
      _bytes[2] = P1;
      _bytes[3] = P2;
      _bytes[4] = P3;
    }

    public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data)
    {
      int i;    
      _bytes = new byte[5 + data.Length];
      _bytes[0] = CLA;
      _bytes[1] = INS;
      _bytes[2] = P1;
      _bytes[3] = P2;
      _bytes[4] = (byte) data.Length;
      for (i = 0; i < data.Length; i++)
        _bytes[5 + i] = data[i];
    }

    public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data)
    {
      int i;
      byte[] _data = (new CardBuffer(data)).GetBytes();
      _bytes = new byte[5 + _data.Length];
      _bytes[0] = CLA;
      _bytes[1] = INS;
      _bytes[2] = P1;
      _bytes[3] = P2;
      _bytes[4] = (byte)_data.Length;
      for (i = 0; i < _data.Length; i++)
        _bytes[5 + i] = _data[i];
    }

    public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data, byte LE)
    {
      int i;
      _bytes = new byte[6 + data.Length];
      _bytes[0] = CLA;
      _bytes[1] = INS;
      _bytes[2] = P1;
      _bytes[3] = P2;
      _bytes[4] = (byte) data.Length;
      for (i = 0; i < data.Length; i++)
        _bytes[5 + i] = data[i];
      _bytes[5 + data.Length] = LE;
    }

    public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data, byte LE)
    {
      int i;
      byte[] _data = (new CardBuffer(data)).GetBytes();
      _bytes = new byte[6 + _data.Length];
      _bytes[0] = CLA;
      _bytes[1] = INS;
      _bytes[2] = P1;
      _bytes[3] = P2;
      _bytes[4] = (byte)_data.Length;
      for (i = 0; i < _data.Length; i++)
        _bytes[5 + i] = _data[i];
      _bytes[5 + _data.Length] = LE;
    }

    public CAPDU(string str)
    {
      SetString(str);
    }

    public byte CLA
    {
      get
      {
        if (_bytes == null)
          return 0xFF;
        return _bytes[0];
      }
      set
      {
        if (_bytes == null)
          _bytes = new byte[4];
        _bytes[0] = value;
      }
    }

    public byte INS
    {
      get
      {
        if (_bytes == null)
          return 0xFF;
        return _bytes[1];
      }
      set
      {
        if (_bytes == null)
          _bytes = new byte[4];
        _bytes[1] = value;
      }
    }

    public byte P1
    {
      get
      {
        if (_bytes == null)
          return 0xFF;
        return _bytes[2];
      }
      set
      {
        if (_bytes == null)
          _bytes = new byte[4];
        _bytes[2] = value;
      }
    }

    public byte P2
    {
      get
      {
        if (_bytes == null)
          return 0xFF;
        return _bytes[3];
      }
      set
      {
        if (_bytes == null)
          _bytes = new byte[4];
        _bytes[3] = value;
      }
    }
    
    private bool hasLc()
    {
      if (!Valid())
        return false;
      if (_bytes.Length <= 5)
        return false;
      return true;
    }

    private bool hasLe()
    {
      if (!Valid())
        return false;
      if (_bytes.Length == 6 + _bytes[4])
        return false;
      return true;
    }
    
    public bool Valid()
    {
      if (_bytes == null)
        return false;
      if (_bytes.Length <= 4)
        return false;
      if (_bytes.Length == 5)
        return true;
      if (_bytes.Length == 5 + _bytes[4])
        return true;
      if (_bytes.Length == 6 + _bytes[4])
        return true;
      return false;
    }

    public byte Lc
    {
      get
      {
        if (!hasLc())
          return 0x00;

        return _bytes[4];
      }
    }

    public byte Le
    {
      get
      {
        if (!hasLe())
          return 0x00;        
        return _bytes[_bytes.Length - 1];
      }

      set
      {
        if (_bytes == null)
          _bytes = new byte[5];
        if (!hasLe())
        {
          byte[] t = new byte[_bytes.Length+1];
          for (int i=0; i<_bytes.Length; i++)
            t[i] = _bytes[i];
          _bytes = t;
        }
        _bytes[_bytes.Length-1] = value;
      }
    }

    public CardBuffer data
    {
      get
      {
        if (!hasLc())
          return null;
        
        byte[] t = new byte[Lc];
        for (int i=0; i<t.Length; i++)
          t[i] = _bytes[5+i];
        
        return new CardBuffer(t);
      }

      set
      {
        int length;
        uint apdu_size;
        
        if (value == null)
          length = 0;
        else
          length = value.Length;
        
        if (length == 0)
        {
          
        } else
        if (length < 256)
        {
          if (hasLe())
            apdu_size = (uint) (6 + length);
          else
            apdu_size = (uint) (5 + length);
          
          byte[] t = new byte[apdu_size];
          
          if (Valid())
          {
            for (int i=0; i<4; i++)
              t[i] = _bytes[i];
            if (hasLe())
              t[t.Length-1] = _bytes[_bytes.Length-1];
          }
          
          for (int i=0; i<length; i++)
            t[5+i] = value.GetByte(i);
          
          t[4] = (byte) length;          
          
          _bytes = t;
        } else
        {
          /* Oups ? */
        }
      }
    }
  }
#endregion

#region RAPDU class

/**c* SpringCardPCSC/RAPDU
 *
 * NAME
 *   RAPDU
 * 
 * DESCRIPTION
 *   The RAPDU object is used to receive and decode RESPONSE APDUs (according to ISO 7816-4) from the smartcard
 * 
 * DERIVED FROM
 *   CardBuffer
 *
 **/

  public class RAPDU:CardBuffer
  {
    public bool isValid
    {
      get
      {
        return (_bytes.Length >= 2);
      }
    }

    public RAPDU(byte[]bytes, int length)
    {
      SetBytes(bytes, length);
    }
    
    public RAPDU(byte[]bytes)
    {
      SetBytes(bytes);
    }
    
    public RAPDU(byte[]bytes, byte SW1, byte SW2)
    {
      byte[] t;
      if (bytes == null)
      {
        t = new byte[2];
        t[0] = SW1;
        t[1] = SW2;
      } else
      {
        t = new byte[bytes.Length + 2];  
        for (int i=0; i<bytes.Length; i++)
          t[i] = bytes[i];
        t[bytes.Length] = SW1;
        t[bytes.Length+1] = SW2;
      }
      SetBytes(t);
    }

    public RAPDU(byte sw1, byte sw2)
    {
      byte[] t = new byte[2];
      t[0] = sw1;
      t[1] = sw2;
      SetBytes(t);
    }

    public RAPDU(ushort sw)
    {
      byte[] t = new byte[2];
      t[0] = (byte) (sw / 0x0100);
      t[1] = (byte) (sw % 0x0100);
      SetBytes(t);
    }

    public bool hasData
    {
      get
      {
        if ((_bytes == null) || (_bytes.Length < 2))
          return false;
          
        return true;
      }
    }

    public CardBuffer data
    {
      get
      {
        if ((_bytes == null) || (_bytes.Length < 2))
          return null;

        return new CardBuffer(_bytes, _bytes.Length - 2);
      }
    }

    public byte SW1
    {
      get
      {
        if ((_bytes == null) || (_bytes.Length < 2))
          return 0xCC;
        return _bytes[_bytes.Length - 2];
      }
    }

    public byte SW2
    {
      get
      {
        if ((_bytes == null) || (_bytes.Length < 2))
          return 0xCC;
        return _bytes[_bytes.Length - 1];
      }
    }

    public ushort SW
    {
      get
      {
        if ((_bytes == null) || (_bytes.Length < 2))
          return 0xCCCC;

        ushort r;

        r = _bytes[_bytes.Length - 2];
        r *= 0x0100;
        r += _bytes[_bytes.Length - 1];

        return r;
      }
    }

    public string SWString
    {
      get
      {
        return SCARD.CardStatusWordsToString(SW1, SW2);
      }
    }
  }
#endregion
}
