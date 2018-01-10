Imports System.Runtime.InteropServices
Imports System.Text
Imports System.IO
Imports System.String
Imports System.Windows.Automation
Imports NDde.Client
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.Net
Imports System.Threading
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.InteropServices.Marshal

Public Class Funcoes_Sistema

    'Criptografia
    Dim textoCifrado As Byte()
    Dim sal() As Byte = {&H0, &H1, &H2, &H3, &H4, &H5, &H6, &H5, &H4, &H3, &H2, &H1, &H0}
    Dim senha As String = ""
    Dim mensagem As String = ""
    Dim CapturaURLThread As New Thread(AddressOf GetChromeCurrentURL2) 'Thread de disparo do csv
    Private TripleDes As New TripleDESCryptoServiceProvider

    Public Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Int32) As Int16

        Private Declare Auto Function FindWindow Lib "user32.dll" (
ByVal lpClassName As String,
ByVal lpWindowName As String
) As IntPtr

        <DllImport("user32.dll", EntryPoint:="GetWindowThreadProcessId")>
        Private Shared Function GetWindowThreadProcessId(<InAttribute()> ByVal hWnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
        End Function

        <DllImport("user32.dll", EntryPoint:="GetForegroundWindow")> Private Shared Function GetForegroundWindow() As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> Private Shared Function GetWindowTextLength(ByVal hwnd As IntPtr) As Integer
        End Function

        <DllImport("user32.dll", EntryPoint:="GetWindowTextW")>
        Private Shared Function GetWindowTextW(<InAttribute()> ByVal hWnd As IntPtr, <OutAttribute(), MarshalAs(UnmanagedType.LPWStr)> ByVal lpString As StringBuilder, ByVal nMaxCount As Integer) As Integer
        End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function LockWorkStation() As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Public ReadOnly Property CurrentExecutionFilePath As String

    'DECLARACAO PARA FORMACAO DO ARQUIVO INI
    Private Declare Auto Function GetPrivateProfileString Lib "Kernel32" (ByVal lpAppName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As StringBuilder, ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    Private Declare Auto Function WritePrivateProfileString Lib "Kernel32" (ByVal lpAppName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer
    'FIM ARQUIVO INI

    Public Sub GetForgroundWindowInfo()

        Dim hWnd As IntPtr = GetForegroundWindow()
        Dim Dados As New Class_Processos


        'Se não for detecada a ociosidade entra
        If Frm_Principal.ChaveDeOciosidade = False Then '###POSICAO 1

            Frm_Principal.Rastreio = "1"

            'Verfica a movimentação da página
            If Not hWnd.Equals(IntPtr.Zero) Then '###POSICAO 2

                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                Dim lgth As Integer = GetWindowTextLength(hWnd)
                Dim wTitle As New System.Text.StringBuilder("", lgth + 1)

                If lgth > 0 Then 'NÃO TEM RESTREIO
                    GetWindowTextW(hWnd, wTitle, wTitle.Capacity)
                End If

                Dim wProcID As Integer = Nothing
                GetWindowThreadProcessId(hWnd, wProcID)

                Dim Proc As Process = Process.GetProcessById(wProcID)
                Dim wFileName As String = ""
                Dim Processo As String = ""
                Dim URLOF As String = ""
                Dim ComboBox1 As New ComboBox

                Try

                    Dados.Processo = Proc.ProcessName

                Catch ex As Exception

                    Dados.Processo = ""

                End Try

                Try

                    Dados.ID = Proc.Id

                Catch ex As Exception

                    Dados.ID = 0

                End Try

                Try

                    Dados.Local = Proc.MainModule.FileName

                Catch ex As Exception

                    Dados.Local = ""

                End Try

                Dados.Nome = wTitle.ToString
                Dados.Chave = Dados.ID & "-" & Dados.Processo & "-" & Dados.Nome
                Dados.ProcessoCompleto = Proc
                Dados.HoraIni = Format(Now, "yyyy-MM-dd HH:mm:ss")

                Dados.URL = ""

                'Ler a URL dos navegadores
                Try



                Catch ex As Exception

                    Dados.URL = "no data"

                End Try

                Dados.Nome = Dados.Nome.Replace(";", "")
                Dados.Local = Dados.Local.Replace(";", "")
                Dados.URL = Dados.URL.Replace(";", "")

                With Frm_Principal

                    If .DadosAtuais.Chave = "Colete" Then

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"
                        .DadosAtuais = Dados

                    Else

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                    End If

                    If .DadosAtuais.Processo = "Walle_Client_Data" Or .DadosAtuais.Processo = "Walle_Client" Or .DadosAtuais.Nome = "Walle" _
                        Or .DadosAtuais.Processo = "4fkhost" Then

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"
                        .DadosAtuais = Dados

                    Else

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                    End If

                    If .DadosAtuais.Chave <> Dados.Chave Then

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                        If .DadosAtuais.Chave = "" Then

                            .DadosAtuais = New Class_Processos
                            .DadosAtuais = Dados
                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                            Exit Sub

                        End If

                        Try

                            If Dados.Processo = "chrome" Then

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"
                                CapturaURLThread = New Thread(AddressOf GetChromeCurrentURL2)
                                CapturaURLThread.Start(Dados)

                            ElseIf Dados.Processo = "iexplorer" Then

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".2"
                                CapturaURLThread = New Thread(AddressOf GetURLIE)
                                CapturaURLThread.Start(Dados)

                                'Dados.URL = GetURLIE(Dados.Nome)

                            ElseIf Dados.Processo = "firefox" Then

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".3"
                                CapturaURLThread = New Thread(AddressOf GetFirefoxUrl)
                                CapturaURLThread.Start(Dados)

                                'URLOF = GetFirefoxUrl()

                            ElseIf Dados.Processo = "ApplicationFrameHost" Then

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".4"
                                CapturaURLThread = New Thread(AddressOf GetURLEDGE)
                                CapturaURLThread.Start(Dados)

                                'Dados.URL = GetURLEDGE(True, True)

                            ElseIf Dados.Processo = "LockApp" Then

                                Dados.Nome = "Bloqueado"
                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                            End If

                        Catch ex As Exception

                            Dados.URL = ""

                        End Try

                        .DadosAtuais.HoraFim = Format(Now, "yyyy-MM-dd HH:mm:ss")
                        .UltimaHoraCapturada = .DadosAtuais.HoraFim

                        .DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(.DadosAtuais.HoraIni),
                                                    CDate(.DadosAtuais.HoraFim)).ToString

                        .DadosAtuais.Rasterar = Frm_Principal.Rastreio
                        Frm_Principal.Rastreio = ""

                        .Lista.Add(.DadosAtuais)

                        .DadosAtuais = New Class_Processos
                        .DadosAtuais = Dados

                    Else

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                        If PosicaoDoMouse() Then

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                            Dim Segundos As Integer

                            Segundos = DateDiff(DateInterval.Second, CDate(.HoraAcionamentoTeclado),
                             CDate(Format(Now, "yyyy-MM-dd HH:mm:ss"))).ToString

                            If Segundos >= 120 Then

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                                .DadosAtuais.HoraFim = Format(Now, "yyyy-MM-dd HH:mm:ss")
                                '.DadosAtuais.HoraFim = CDate(.DadosAtuais.HoraFim).AddSeconds(-30)

                                .UltimaHoraCapturada = .DadosAtuais.HoraFim

                                .DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(.DadosAtuais.HoraIni),
                                                              CDate(.DadosAtuais.HoraFim)).ToString

                                .DadosAtuais.Rasterar = Frm_Principal.Rastreio
                                Frm_Principal.Rastreio = ""

                                .Lista.Add(.DadosAtuais)

                                Dados.ID = 0
                                Dados.Nome = "Ociosidade"
                                Dados.Local = "Ociosidade"
                                Dados.Processo = "Ociosidade"
                                Dados.Chave = Dados.ID & "-" & Dados.Processo & "-" & Dados.Nome
                                Dados.HoraIni = .DadosAtuais.HoraFim

                                .DadosAtuais = New Class_Processos
                                .DadosAtuais = Dados

                                Frm_Principal.ChaveDeOciosidade = True

                            Else

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                            End If

                        Else

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                        End If

                    End If

                End With

            Else

                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                With Frm_Principal

                    If PosicaoDoMouse() Then

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                        Dim Segundos As Integer

                        Segundos = DateDiff(DateInterval.Second, CDate(.HoraAcionamentoTeclado),
                         CDate(Format(Now, "yyyy-MM-dd HH:mm:ss"))).ToString

                        If Segundos >= 120 Then

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                            .DadosAtuais.HoraFim = Format(Now, "yyyy-MM-dd HH:mm:ss")
                            '.DadosAtuais.HoraFim = CDate(.DadosAtuais.HoraFim).AddSeconds(-30)

                            .UltimaHoraCapturada = .DadosAtuais.HoraFim

                            .DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(.DadosAtuais.HoraIni),
                                                                  CDate(.DadosAtuais.HoraFim)).ToString

                            .DadosAtuais.Rasterar = Frm_Principal.Rastreio
                            Frm_Principal.Rastreio = ""

                            .Lista.Add(.DadosAtuais)

                            Dados.ID = 0
                            Dados.Nome = "Ociosidade"
                            Dados.Local = "Ociosidade"
                            Dados.Processo = "Ociosidade"
                            Dados.Chave = Dados.ID & "-" & Dados.Processo & "-" & Dados.Nome
                            Dados.HoraIni = .DadosAtuais.HoraFim

                            .DadosAtuais = New Class_Processos
                            .DadosAtuais = Dados

                            Frm_Principal.ChaveDeOciosidade = True

                        Else

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                            If .DadosAtuais.Chave = "Colete" Then

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                                .DadosAtuais.ID = 0
                                .DadosAtuais.Nome = "Ociosidade"
                                .DadosAtuais.Local = "Ociosidade"
                                .DadosAtuais.Processo = "Ociosidade"
                                .DadosAtuais.Chave = .DadosAtuais.ID & "-" & .DadosAtuais.Processo & "-" & .DadosAtuais.Nome

                            Else

                                Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                            End If

                        End If

                    Else

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                    End If

                End With

            End If

        Else

            Frm_Principal.Rastreio = "0"

            With Frm_Principal

                If PosicaoDoMouse() Then

                    Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                    Dim Segundos As Integer

                    Segundos = DateDiff(DateInterval.Second, CDate(.HoraAcionamentoTeclado),
                    CDate(Format(Now, "yyyy-MM-dd HH:mm:ss"))).ToString

                    If Segundos < 120 Then

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                        If .DadosAtuais.Processo = "" Then

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                            .DadosAtuais.ID = 0
                            .DadosAtuais.Nome = "Ociosidade"
                            .DadosAtuais.Local = "Ociosidade"
                            .DadosAtuais.Processo = "Ociosidade"
                            .DadosAtuais.Chave = .DadosAtuais.ID & "-" & .DadosAtuais.Processo & "-" & .DadosAtuais.Nome

                        Else

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                        End If

                        .DadosAtuais.HoraFim = Format(Now, "yyyy-MM-dd HH:mm:ss")
                        .UltimaHoraCapturada = .DadosAtuais.HoraFim

                        .DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(.DadosAtuais.HoraIni),
                                                      CDate(.DadosAtuais.HoraFim)).ToString

                        .DadosAtuais.Rasterar = Frm_Principal.Rastreio
                        Frm_Principal.Rastreio = ""

                        .Lista.Add(.DadosAtuais)

                        .DadosAtuais = New Class_Processos
                        .DadosAtuais.Chave = "Colete"

                        Frm_Principal.Clep = False
                        Frm_Principal.ChaveDeOciosidade = False

                    Else

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                        If .DadosAtuais.Chave = "Colete" Then

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                            .DadosAtuais.ID = 0
                            .DadosAtuais.Nome = "Ociosidade"
                            .DadosAtuais.Local = "Ociosidade"
                            .DadosAtuais.Processo = "Ociosidade"
                            .DadosAtuais.Chave = .DadosAtuais.ID & "-" & .DadosAtuais.Processo & "-" & .DadosAtuais.Nome
                            'MsgBox(.DadosAtuais.HoraIni)

                        Else

                            Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                        End If

                    End If

                Else

                    Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                    If .DadosAtuais.Processo = "" Then

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".1"

                        .DadosAtuais.ID = 0
                        .DadosAtuais.Nome = "Ociosidade"
                        .DadosAtuais.Local = "Ociosidade"
                        .DadosAtuais.Processo = "Ociosidade"
                        .DadosAtuais.Chave = .DadosAtuais.ID & "-" & .DadosAtuais.Processo & "-" & .DadosAtuais.Nome

                    Else

                        Frm_Principal.Rastreio = Frm_Principal.Rastreio & ".0"

                    End If

                    .DadosAtuais.HoraFim = Format(Now, "yyyy-MM-dd HH:mm:ss")
                    .UltimaHoraCapturada = .DadosAtuais.HoraFim

                    .DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(.DadosAtuais.HoraIni),
                                                  CDate(.DadosAtuais.HoraFim)).ToString

                    .DadosAtuais.Rasterar = Frm_Principal.Rastreio
                    Frm_Principal.Rastreio = ""

                    .Lista.Add(.DadosAtuais)

                    .DadosAtuais = New Class_Processos
                    .DadosAtuais.Chave = "Colete"

                    Frm_Principal.ChaveDeOciosidade = False

                End If

            End With

        End If

    End Sub

    Public Function GerarArquivo(ByVal ListadeDados As List(Of Class_Processos)) As Boolean

        Dim chave As New Rfc2898DeriveBytes(senha, sal)
        ' criptografa os dados
        Dim algoritmo = New RijndaelManaged()
        Dim Criptografia4k As String
        Dim UsernameSem As String = UCase(Environ("USERNAME"))
        Dim fluxoTexto2 As IO.StreamWriter
        Dim FecharCiclo As Boolean = False
        Dim NovoProcesso As New Class_Processos
        Dim MeuArray As New ArrayList
        Dim IPClient As String = ""
        Dim HostnameClient As String = ""
        Dim CodUs As String = ""
        Dim LerArquivo As IO.StreamReader

        algoritmo.Key = chave.GetBytes(16)
        algoritmo.IV = chave.GetBytes(16)

        Dim IPList As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName)
        Dim IP As IPAddress
        For Each IP In IPList.AddressList
            'Only return IPv4 routable IPs
            If (IP.AddressFamily = Sockets.AddressFamily.InterNetwork) Then
                IPClient = IP.ToString
                Exit For

            End If
        Next

        If File.Exists(Frm_Principal.ClientLocalPasta & "\BIN.txt") = True Then

            LerArquivo = New IO.StreamReader(Frm_Principal.ClientLocalPasta & "\BIN.txt")
            CodUs = LerArquivo.ReadLine

            LerArquivo.Close()

        End If

        MeuArray.Add(CodUs)
        MeuArray.Add(Environment.MachineName)
        MeuArray.Add(IPClient)

        Try

            If descarregarArquivo2("ftp://ftp.fourkey.com.br", Frm_Principal.UserCript,
                                         Frm_Principal.PassCript, MeuArray, Frm_Principal.CodClienteWalle) = False Then

                FecharCiclo = True
                Frm_Principal.Timer_ColetaDados.Enabled = False
                Frm_Principal.Timer_Licenca.Enabled = True

                NovoProcesso.Chave = "Realocação de Licença"
                NovoProcesso.Local = "Realocação de Licença"
                NovoProcesso.Nome = "Realocação de Licença"
                NovoProcesso.Processo = "Realocação de Licença"
                NovoProcesso.Tempo = "00:00:00"
                NovoProcesso.URL = ""
                NovoProcesso.HoraIni = ListadeDados.Item(ListadeDados.Count - 1).HoraFim
                NovoProcesso.HoraFim = NovoProcesso.HoraIni

            Else

                FecharCiclo = False
                Frm_Principal.Timer_ColetaDados.Enabled = True

            End If

        Catch ex As Exception

            fluxoTexto2 = New IO.StreamWriter(Frm_Principal.ClientLocalPasta & "\Logs\Admin_Walle.txt", True)
            fluxoTexto2.WriteLine("------------------------")
            fluxoTexto2.WriteLine(Format(Now, "yyyy-MM-dd HH:mm:ss") & ": (" & Frm_Principal.MeuArray.Item(1) & " - " _
                                 & Frm_Principal.MeuArray.Item(2) & ")" & ex.Message)
            fluxoTexto2.Close()

            Application.Exit()

        End Try

        'Verifica se existem dados duplicados e retorna a lista sem duplicações
        ListadeDados = VerificarDuplicados(ListadeDados)

        If FecharCiclo = True Then

            ListadeDados.Add(NovoProcesso)

        End If

        Try

            Dim Linha As Object = ""

            If ListadeDados.Count = 0 Then

                Return True
                Exit Function

            End If

            For i As Integer = 0 To ListadeDados.Count - 1

                If ListadeDados.Item(i).Processo = "chrome" Or ListadeDados.Item(i).Processo = "iexplorer" Or ListadeDados.Item(i).Processo = "firefox" _
                    Or ListadeDados.Item(i).Processo = "ApplicationFrameHost" Then

                    ListadeDados.Item(i).URL = ProcurarURLAdrees(CDate(ListadeDados.Item(i).HoraIni).ToString("yyyy-MM-dd-HH-mm-ss") _
                                                                 & "_" & ListadeDados.Item(i).ID & ".4fk", Frm_Principal.ClientFourkey)

                End If

                ListadeDados.Item(i).Processo = ListadeDados.Item(i).Processo.Replace("'", "")
                ListadeDados.Item(i).Nome = ListadeDados.Item(i).Nome.Replace("'", "")
                ListadeDados.Item(i).Local = ListadeDados.Item(i).Local.Replace("'", "")

                Try

                    ListadeDados.Item(i).URL = ListadeDados.Item(i).URL.Replace("'", "")
                    ListadeDados.Item(i).URL = ListadeDados.Item(i).URL.Replace(",", "")
                    ListadeDados.Item(i).URL = ListadeDados.Item(i).URL.Replace("’", "")

                Catch ex As Exception

                    ListadeDados.Item(i).URL = ""

                End Try

                ListadeDados.Item(i).Processo = ListadeDados.Item(i).Processo.Replace(",", "")
                ListadeDados.Item(i).Nome = ListadeDados.Item(i).Nome.Replace(",", "")
                ListadeDados.Item(i).Local = ListadeDados.Item(i).Local.Replace(",", "")


                ListadeDados.Item(i).Processo = ListadeDados.Item(i).Processo.Replace("’", "")
                ListadeDados.Item(i).Nome = ListadeDados.Item(i).Nome.Replace("’", "")
                ListadeDados.Item(i).Local = ListadeDados.Item(i).Local.Replace("’", "")

                UsernameSem = UsernameSem.Replace("'", "")
                UsernameSem = UsernameSem.Replace(",", "")
                UsernameSem = UsernameSem.Replace("’", "")

                Criptografia4k = UsernameSem & ";" _
                & ListadeDados.Item(i).Processo & ";" _
                & ListadeDados.Item(i).Nome & ";" _
                & ListadeDados.Item(i).Local & ";" _
                & ListadeDados.Item(i).HoraIni & ";" _
                    & ListadeDados.Item(i).HoraFim & ";" _
                    & ListadeDados.Item(i).Tempo & ";" _
                    & ListadeDados.Item(i).URL & ";" _
                    & Frm_Principal.EnderecoIPProcessador & ";" _
                 & Frm_Principal.ClientFourkey & ";" _
                 & Mid(ListadeDados.Item(i).Chave, 1, 250) & ";" _
                 & Frm_Principal.Versao & ";" _
                & ListadeDados.Item(i).Rasterar & ";"

                Criptografia4k = Criptografar(Criptografia4k, Frm_Principal.ClientFourkey)

                Linha = Linha & Criptografia4k & vbCrLf

            Next

            If Not Directory.Exists(Frm_Principal.ClientLocation) Then

                Try

                    Directory.CreateDirectory(Frm_Principal.ClientLocation)

                Catch ex As Exception

                    MsgBox("Walle: Atenção, você não tem permissão de gravação. Por favor entre em contato com os " _
                        & "administradores do sistema", vbExclamation)

                End Try

            End If


            Dim Arquivo As String = Frm_Principal.ClientLocation & "\WalleClient_" & UCase(Environ("USERNAME")) & "_"
            Dim DataAgora As Date = Now

            Arquivo = Arquivo & DataAgora.Year & "-" & DataAgora.Month.ToString.PadLeft(2, "0") _
                & "-" & DataAgora.Day.ToString.PadLeft(2, "0") & "-" & DataAgora.Hour.ToString.PadLeft(2, "0") _
                & "-" & DataAgora.Minute.ToString.PadLeft(2, "0") & "-" & DataAgora.Second.ToString.PadLeft(2, "0") _
             & ".4fk"

            mensagem = Linha

            Dim fonteBytes() As Byte = New System.Text.UnicodeEncoding().GetBytes(mensagem)

            Using StreamFonte = New MemoryStream(fonteBytes)
                Using StreamDestino As New MemoryStream()
                    Using crypto As New CryptoStream(StreamFonte, algoritmo.CreateEncryptor(), CryptoStreamMode.Read)
                        moveBytes(crypto, StreamDestino)
                        textoCifrado = StreamDestino.ToArray()
                    End Using
                End Using
            End Using

            Dim fluxoTexto As IO.StreamWriter
            fluxoTexto = New IO.StreamWriter(Arquivo, True)
            fluxoTexto.WriteLine(Linha)
            fluxoTexto.Close()

            Dim CaminhoWi As String = System.Reflection.Assembly.GetExecutingAssembly().Location

            CaminhoWi = CaminhoWi.Replace("Walle_Client.exe", "")

            If File.Exists(CaminhoWi & "Windows64.txt") Then

                Frm_Auxiliar.Fechamento = True

                For Each processo As Process In Process.GetProcesses()

                    If processo.ProcessName = "Walle_Client_Data" Then

                        processo.Kill()

                    End If

                Next processo

                Application.Exit()

            End If

            Return True

        Catch ex As Exception

            Return False

        End Try

    End Function

    Private Function ProcurarURLAdrees(ByVal Seleciona As String, ByVal ChaveStr As String)

        Dim URL As String = ""
        Dim fluxoTexto As IO.StreamReader
        Dim Tratado As String
        Dim chave As New Rfc2898DeriveBytes(ChaveStr, sal)
        Dim algoritmo = New RijndaelManaged()
        Dim Limpa As String = ""
        Dim Doisdias As Date = Now
        Dim Outro As String
        Dim ChaveFinal As String

        Try

            For Each arquivo In Directory.GetFiles(Frm_Principal.ClientLocation & "\Address")

                Tratado = arquivo.Replace(Frm_Principal.ClientLocation & "\Address\", "")
                ChaveFinal = Tratado

                If Tratado = Seleciona.ToString Then

                    If IO.File.Exists(Frm_Principal.ClientLocation & "\Address\" & Seleciona) Then

                        fluxoTexto = New IO.StreamReader(Frm_Principal.ClientLocation & "\Address\" & Seleciona)
                        URL = fluxoTexto.ReadLine

                        textoCifrado = Convert.FromBase64String(URL)

                        algoritmo.Key = chave.GetBytes(16)
                        algoritmo.IV = chave.GetBytes(16)

                        Using StreamFonte = New MemoryStream(textoCifrado)

                            Using StreamDestino As New MemoryStream()

                                Using crypto As New CryptoStream(StreamFonte, algoritmo.CreateDecryptor(), CryptoStreamMode.Read)

                                    moveBytes(crypto, StreamDestino)

                                    Dim bytesDescriptografados() As Byte = StreamDestino.ToArray()
                                    Dim mensagemDescriptografada = New UnicodeEncoding().GetString(bytesDescriptografados)

                                    URL = mensagemDescriptografada

                                End Using

                            End Using

                        End Using

                        fluxoTexto.Close()

                        Try

                            My.Computer.FileSystem.DeleteFile(Frm_Principal.ClientLocation & "\Address\" & Seleciona,
                                                          FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

                        Catch ex As Exception



                        End Try

                    End If

                End If

                Outro = Mid(Tratado, 12, 8)
                Tratado = Mid(Tratado, 1, 10)

                Doisdias = Now
                Doisdias = Doisdias.AddDays(-1)
                Tratado = Tratado.Replace("-", "/")
                Outro = Outro.Replace("-", ":")
                Tratado = Tratado & " " & Outro

                If CDate(Tratado) <= Doisdias.ToString("yyyy/MM/dd HH:mm:ss") Then

                    Try

                        My.Computer.FileSystem.DeleteFile(Frm_Principal.ClientLocation & "\Address\" & ChaveFinal,
                                                          FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

                    Catch ex As Exception


                    End Try


                End If

            Next

        Catch ex As Exception

            URL = ""

        End Try

        Return URL

    End Function

    Public Function VerificarDuplicados(ByVal ListadeDados As List(Of Class_Processos))

        Dim ListaNova As New List(Of Class_Processos)
        Dim Achou As Boolean = False

        For i As Integer = 0 To ListadeDados.Count - 1

            Achou = False

            For j As Integer = 0 To ListaNova.Count - 1

                If ListadeDados.Item(i).HoraIni = ListaNova.Item(j).HoraIni And ListadeDados.Item(i).HoraFim = ListaNova.Item(j).HoraFim Then

                    Achou = True

                End If

            Next

            If Achou = False Then

                ListaNova.Add(ListadeDados.Item(i))

            End If

        Next

        Return ListaNova

    End Function

    Private Sub moveBytes(ByVal fonte As Stream, ByVal destino As Stream)
        Dim bytes(2048) As Byte
        Dim contador = fonte.Read(bytes, 0, bytes.Length - 1)
        While (0 <> contador)
            destino.Write(bytes, 0, contador)
            contador = fonte.Read(bytes, 0, bytes.Length - 1)
        End While
    End Sub

    Private Function PosicaoDoMouse() As Boolean

        If Cursor.Position.X <> Frm_Principal.MouseX And Cursor.Position.Y <> Frm_Principal.MouseY Then

                Frm_Principal.MouseX = Cursor.Position.X
                Frm_Principal.MouseY = Cursor.Position.Y

                Frm_Principal.HoraMouseOciso = Format(Now, "yyyy-MM-dd HH: mm:ss")

                Return False

            Else

                Frm_Principal.TempoMouseOcioso = DateDiff(DateInterval.Second, CDate(Frm_Principal.HoraMouseOciso),
                                                  CDate(Format(Now, "yyyy-MM-dd HH:mm:ss"))).ToString

            If Frm_Principal.TempoMouseOcioso >= 120 Then

                Return True

            Else

                Return False

                End If

            End If

        End Function

    Public Function AcionamentoDoTeclado(ByVal Letra As String) As String

        For logger = 1 To 255

            If GetAsyncKeyState(logger) = -32767 Then

                If Frm_Principal.ContTecla = 0 And logger = 17 Then

                    Frm_Principal.ContTecla = 1

                ElseIf Frm_Principal.ContTecla = 1 And logger = 104 Then

                    Frm_Principal.ContTecla = 2

                ElseIf Frm_Principal.ContTecla = 2 And logger = 105 Then

                    Frm_Principal.ContTecla = 0
                    Frm_Auxiliar.Show()
                    Frm_Auxiliar.WindowState = FormWindowState.Normal
                    Frm_Auxiliar.ShowInTaskbar = True

                ElseIf Frm_Principal.ContTecla = 1 And logger <> 104 Then

                    Frm_Principal.ContTecla = 0

                ElseIf Frm_Principal.ContTecla = 2 And logger <> 105 Then

                    Frm_Principal.ContTecla = 0

                End If

                Return Format(Now, "yyyy-MM-dd HH:mm:ss")
                Exit Function

            End If

        Next

        Return ""

    End Function

    Public Sub GetURLIE(ByVal Dados As Class_Processos)

        Dim SWS As New SHDocVw.ShellWindows
        Dim IE As New SHDocVw.InternetExplorer
        Dim URL As String = ""
        Dim Titulo As String = Dados.Nome
        Dim ProcRecebida As New Class_Processos

        For Each IE In SWS
            ' O ShellWindows pega também as janelas do Windows Explorer,
            ' portanto separe-as das do IE
            If LCase(Right(IE.FullName, 12)) = "iexplore.exe" Then

                Try

                    If Titulo = IE.Document.Title & " - Internet Explorer" Then

                        URL = IE.LocationURL      ' URL

                        Exit For

                    End If

                Catch ex As Exception

                End Try

            End If

        Next IE

        Dados.URL = URL
        ProcRecebida = Dados

        RegistrarTXT(ProcRecebida)

    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function FindWindowEx(parentHandle As IntPtr, childAfter As IntPtr, className As String, windowTitle As String) As IntPtr
    End Function

    Private Const ChromeProcess As [String] = "chrome"
    Private Const AddressCtl As [String] = "Address and search bar"

    Public Shared Function GetChromeHandle() As IntPtr
        Dim ChromeHandle As IntPtr = Nothing
        Dim Allpro() As Process = Process.GetProcesses()
        For Each pro As Process In Allpro
            If pro.ProcessName = "chrome" Then
                ChromeHandle = pro.MainWindowHandle
                Exit For
            End If
        Next
        Return ChromeHandle
    End Function

    Public Function GetCurrentUrlChrome2(ByVal chrome As Process) As String
        ' the chrome process must have a window
        If chrome.MainWindowHandle = IntPtr.Zero Then
            Return String.Empty
            Exit Function
        End If
        'AutomationElement elm = AutomationElement.RootElement.FindFirst(TreeScope.Children,
        '         new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1"));
        ' find the automation element
        Dim elm As AutomationElement = AutomationElement.FromHandle(chrome.MainWindowHandle)

        ' manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
        Dim elmUrlBar As AutomationElement = Nothing
        'try
        '{
        ' walking path found using inspect.exe (Windows SDK) for Chrome 29.0.1547.76 m (currently the latest stable)
        Dim elm1 = elm.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "Google Chrome"))
        If elm1 Is Nothing Then
            Return String.Empty
            Exit Function
        End If
        ' not the right chrome.exe
        Dim elm2 = TreeWalker.RawViewWalker.GetLastChild(elm1)
        Dim elm3 = GetChildByIndex(elm2, 1)
        Dim elm4 = elm3.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "main"))
        Dim elm5 = GetChildByIndex(elm4, 3)
        elmUrlBar = elm5.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))
        '}
        'catch
        '{
        '    // Chrome has probably changed something, and above walking needs to be modified. :(
        '    // put an assertion here or something to make sure you don't miss it
        '    continue;
        '}

        ' make sure it's valid
        If elmUrlBar Is Nothing Then
            ' it's not..
            Return String.Empty
            Exit Function
        End If

        ' elmUrlBar is now the URL bar element. we have to make sure that it's out of keyboard focus if we want to get a valid URL
        If CBool(elmUrlBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) Then
            Return String.Empty
            Exit Function
        End If

        ' there might not be a valid pattern to use, so we have to make sure we have one
        Dim patterns As AutomationPattern() = elmUrlBar.GetSupportedPatterns()
        If patterns.Length = 1 Then
            Dim ret As String = ""
            Try
                ret = DirectCast(elmUrlBar.GetCurrentPattern(patterns(0)), ValuePattern).Current.Value
            Catch
            End Try
            If ret <> "" Then
                ' must match a domain name (and possibly "https://" in front)
                If Regex.IsMatch(ret, "^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$") Then
                    ' prepend http:// to the url, because Chrome hides it if it's not SSL
                    If Not ret.StartsWith("http") Then
                        ret = "http://" & ret
                    End If
                    Return ret
                End If
            End If
            Return String.Empty
            Exit Function
        End If

        Return String.Empty

    End Function

    Public Function GetChromeCurrentURL(ByVal Processo As Process) As String

        Dim procsChrome As Process() = Process.GetProcessesByName("chrome")

        For Each chrome As Process In procsChrome
            ' the chrome process must have a window
            If chrome.MainWindowHandle = IntPtr.Zero Then
                Continue For
            End If

            chrome = Processo
            'AutomationElement elm = AutomationElement.RootElement.FindFirst(TreeScope.Children,
            '         new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1"));
            ' find the automation element
            Dim elm As AutomationElement = AutomationElement.FromHandle(chrome.MainWindowHandle)

            ' manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
            Dim elmUrlBar As AutomationElement = Nothing
            'try
            '{
            ' walking path found using inspect.exe (Windows SDK) for Chrome 29.0.1547.76 m (currently the latest stable)
            Dim elm1 = elm.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "Google Chrome"))
            If elm1 Is Nothing Then
                Continue For
            End If
            ' not the right chrome.exe
            Dim elm2 = TreeWalker.RawViewWalker.GetLastChild(elm1)
            Dim elm3 = GetChildByIndex(elm2, 1)
            Dim elm4 = elm3.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "main"))
            Dim elm5 = GetChildByIndex(elm4, 3)
            elmUrlBar = elm5.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))
            '}
            'catch
            '{
            '    // Chrome has probably changed something, and above walking needs to be modified. :(
            '    // put an assertion here or something to make sure you don't miss it
            '    continue;
            '}

            ' make sure it's valid
            If elmUrlBar Is Nothing Then
                ' it's not..
                Continue For
            End If

            ' elmUrlBar is now the URL bar element. we have to make sure that it's out of keyboard focus if we want to get a valid URL
            If CBool(elmUrlBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) Then
                Continue For
            End If

            ' there might not be a valid pattern to use, so we have to make sure we have one
            Dim patterns As AutomationPattern() = elmUrlBar.GetSupportedPatterns()
            If patterns.Length = 1 Then
                Dim ret As String = ""
                Try
                    ret = DirectCast(elmUrlBar.GetCurrentPattern(patterns(0)), ValuePattern).Current.Value
                Catch
                End Try
                If ret <> "" Then
                    ' must match a domain name (and possibly "https://" in front)
                    If Regex.IsMatch(ret, "^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$") Then
                        ' prepend http:// to the url, because Chrome hides it if it's not SSL
                        If Not ret.StartsWith("http") Then
                            ret = "http://" & ret
                        End If
                        Return ret
                    End If
                End If
                Continue For
            End If
        Next
        Return String.Empty
    End Function

    Public Function GetChromeCurrentURL2(ByVal ProcRecebida As Class_Processos) As String

        'AutomationElement elm = AutomationElement.RootElement.FindFirst(TreeScope.Children,
        '         new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1"));
        ' find the automation element
        Dim elm As AutomationElement

        Try

            elm = AutomationElement.FromHandle(ProcRecebida.ProcessoCompleto.MainWindowHandle)

        Catch ex As Exception

            Thread.Sleep(1000)
            elm = AutomationElement.FromHandle(ProcRecebida.ProcessoCompleto.MainWindowHandle)

        End Try

        ' manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
        Dim elmUrlBar As AutomationElement = Nothing
        'try
        '{
        ' walking path found using inspect.exe (Windows SDK) for Chrome 29.0.1547.76 m (currently the latest stable)
        Dim elm1

        Try

            elm1 = elm.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "Google Chrome"))

        Catch ex As Exception

            Thread.Sleep(1000)
            elm1 = elm.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "Google Chrome"))

        End Try

        If elm1 Is Nothing Then
            Return ""
            Exit Function
        End If
        ' not the right chrome.exe

        Try

            Dim elm2 = TreeWalker.RawViewWalker.GetLastChild(elm1)
            Dim elm3 = GetChildByIndex(elm2, 1)
            Dim elm4 = elm3.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.NameProperty, "main"))
            Dim elm5 = GetChildByIndex(elm4, 3)
            elmUrlBar = elm5.FindFirst(TreeScope.Children, New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))

        Catch ex As Exception

            Return ""
            Exit Function

        End Try

        '}
        'catch
        '{
        '    // Chrome has probably changed something, and above walking needs to be modified. :(
        '    // put an assertion here or something to make sure you don't miss it
        '    continue;
        '}

        ' make sure it's valid
        If elmUrlBar Is Nothing Then
            ' it's not..
            Return ""
            Exit Function
        End If

        ' elmUrlBar is now the URL bar element. we have to make sure that it's out of keyboard focus if we want to get a valid URL
        If CBool(elmUrlBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) Then
            Return ""
            Exit Function
        End If

        ' there might not be a valid pattern to use, so we have to make sure we have one
        Dim patterns As AutomationPattern() = elmUrlBar.GetSupportedPatterns()
        If patterns.Length = 1 Then
            Dim ret As String = ""
            Try
                ret = DirectCast(elmUrlBar.GetCurrentPattern(patterns(0)), ValuePattern).Current.Value
            Catch
            End Try
            If ret <> "" Then
                ' must match a domain name (and possibly "https://" in front)
                If Regex.IsMatch(ret, "^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$") Then
                    ' prepend http:// to the url, because Chrome hides it if it's not SSL
                    If Not ret.StartsWith("http") Then
                        ret = "http://" & ret
                    End If

                    ProcRecebida.URL = ret
                    RegistrarTXT(ProcRecebida)

                    'Return ret
                End If
            End If
            Return ""
            Exit Function
        End If

        'Return String.Empty
        ProcRecebida.URL = String.Empty
        RegistrarTXT(ProcRecebida)

        Return ""
    End Function

    Public Sub RegistrarTXT(ByVal Dados As Class_Processos)

        Dim fluxoTexto As IO.StreamWriter
        Dim Linha As String

        Linha = CDate(Dados.HoraIni).ToString("yyyy-MM-dd-HH-mm-ss")
        Linha = Linha.Replace("/", "-")
        Linha = Linha.Replace(":", "-")
        Linha = Linha.Replace(" ", "-")

        fluxoTexto = New IO.StreamWriter(Frm_Principal.ClientLocation & "\Address\" & Linha & "_" & Dados.ID & ".4fk", True)
        Dados.URL = Criptografar(Dados.URL, Frm_Principal.ClientFourkey)
        fluxoTexto.WriteLine(Dados.URL)
        fluxoTexto.Close()

        SetAttr(Frm_Principal.ClientLocation & "\Address\" & Linha & "_" & Dados.ID & ".4fk", vbHidden)

    End Sub

    Private Function GetChildByIndex(Root As AutomationElement, index As Integer) As AutomationElement
        Dim current As AutomationElement = Nothing
        For i As Integer = 0 To (index + 1) - 1
            If current Is Nothing Then
                current = TreeWalker.RawViewWalker.GetFirstChild(Root)
            Else
                current = TreeWalker.RawViewWalker.GetNextSibling(current)
            End If
        Next
        Return current
    End Function

    Public Sub GetFirefoxUrl(ByVal Dados As Class_Processos)
        Dim dde As New DdeClient("Firefox", "WWW_GetWindowInfo")
        dde.Connect()
        Dim url As String = dde.Request("URL", Integer.MaxValue)
        dde.Disconnect()
        Dim NovaURL As String = ""
        Dim ProcRecebida As New Class_Processos

        For i As Integer = 0 To url.Length - 1

            If url.Chars(i) = "," Then

                Exit For

            Else

                NovaURL = NovaURL & url.Chars(i)

            End If

        Next

        NovaURL = NovaURL.Replace("""", "")
        Dados.URL = NovaURL

        ProcRecebida = Dados

        RegistrarTXT(ProcRecebida)

    End Sub

    Private Sub GetURLEDGE(ByVal Dados As Class_Processos)

        Dim HadError As Boolean = True
        Dim InhibitMsgBox As Boolean = True

        Dim i1 As Integer
        Dim tmp1 As String = "", tmp2() As String, METitle As String, MEURL As String = ""
        Dim strME As String = "Microsoft Edge"
        Dim ProcRecebida As New Class_Processos

        'ActiveMicrosoftEdgeTitleAndURL(Index) = Page Title or "No Title" + Chr(255) + Page URL

        'If no Page URL then any Page Title is ignored.
        '   If the form is minimized to the taskbar the url is typically not available.

        HadError = False : ReDim tmp2(-1) : i1 = -1

        Try
            Dim conditions As Condition = Condition.TrueCondition
            Dim BaseElement As AutomationElement = AutomationElement.RootElement
            Dim elementCollection As AutomationElementCollection = BaseElement.FindAll(TreeScope.Children, conditions)
            Dim AE As AutomationElement
            For Each AE In elementCollection
                If AE IsNot Nothing Then
                    tmp1 = AE.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString
                    If StrComp(Strings.Right(tmp1, strME.Length), strME, vbTextCompare) = 0 Then
                        MEURL = "" : METitle = ""
                        '-----------------------------------------------------------------------------------------------------------
                        Dim AE1 As AutomationElement =
                            AE.FindFirst(TreeScope.Subtree, New PropertyCondition(AutomationElement.AutomationIdProperty, "TitleBar"))
                        METitle = AutomationElementText(AE1)
                        'METitle = Trim(METitle)
                        '-----------------------------------------------------------------------------------------------------------
                        AE1 = AE.FindFirst(TreeScope.Subtree, New PropertyCondition(AutomationElement.AutomationIdProperty, "addressEditBox"))
                        MEURL = AutomationElementText(AE1)
                        'MEURL = Trim(MEURL)
                        '-----------------------------------------------------------------------------------------------------------
                        If MEURL <> "" Then
                            If METitle = "" Then METitle = "No Title"
                            i1 = i1 + 1 : Array.Resize(tmp2, i1 + 1)
                            tmp2(i1) = METitle + Chr(255) + MEURL
                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            HadError = True
        End Try

        Dados.URL = MEURL
        ProcRecebida = Dados

        RegistrarTXT(ProcRecebida)

    End Sub
    Private Function AutomationElementText(ByRef AE As AutomationElement) As String

        Dim MyPattern As AutomationPattern = ValuePattern.Pattern
        Dim MyPattern1 As AutomationPattern = TextPattern.Pattern
        Dim objPattern As Object = Nothing
        Dim txt As String = ""

        'Any error just return a null string. !r

        If AE.TryGetCurrentPattern(MyPattern, objPattern) Then
            Dim AEValuePattern As ValuePattern = AE.GetCurrentPattern(MyPattern)
            txt = AEValuePattern.Current.Value
        Else
            If AE.TryGetCurrentPattern(MyPattern1, objPattern) Then
                Dim AETextPattern As TextPattern = AE.GetCurrentPattern(MyPattern1)
                txt = AETextPattern.DocumentRange.GetText(-1)
            End If
        End If

        Return txt

    End Function

    Public Function descarregarArquivo(ByVal arquivoFTP As String,
                            ByVal usuario As String, ByVal senha As String,
                            ByVal dirLocal As String) As String

        Dim localFile As String = System.Reflection.Assembly.GetExecutingAssembly().Location & "yek.txt"
        Dim remoteFile As String = "/Walle/Client/" & dirLocal
        Dim host As String = arquivoFTP
        Dim fluxoTexto As IO.StreamReader
        Dim linhaTexto As String

        'Create a request
        Dim URI As String = host & remoteFile
        Dim ftp As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(URI), System.Net.FtpWebRequest)
        'Set the credentials
        ftp.Credentials = New System.Net.NetworkCredential(usuario, senha)
        'Turn off KeepAlive (will close connection on completion)
        ftp.KeepAlive = False
        'we want a binary
        ftp.UseBinary = True
        'Define the action required (in this case, download a file)
        ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile

        'If we were using a method that uploads data e.g. 3
        'we would open the ftp.GetRequestStream here an send the data

        'Get the response to the Ftp request and the associated stream
        Using response As System.Net.FtpWebResponse = CType(ftp.GetResponse, System.Net.FtpWebResponse)
            Using responseStream As IO.Stream = response.GetResponseStream
                'loop to read & write to file
                Using fs As New IO.FileStream(localFile, IO.FileMode.Create)
                    Dim buffer(2047) As Byte
                    Dim read As Integer = 0
                    Do
                        read = responseStream.Read(buffer, 0, buffer.Length)
                        fs.Write(buffer, 0, read)
                    Loop Until read = 0 'see Note(1)
                    responseStream.Close()
                    fs.Flush()
                    fs.Close()
                End Using
                responseStream.Close()
            End Using
            response.Close()
        End Using

        fluxoTexto = New IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().Location & "yek.txt")
        linhaTexto = fluxoTexto.ReadLine
        fluxoTexto.Close()

        My.Computer.FileSystem.DeleteFile(System.Reflection.Assembly.GetExecutingAssembly().Location & "yek.txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

        Return linhaTexto

    End Function

    Public Function descarregarArquivo2(ByVal arquivoFTP As String,
                            ByVal usuario As String, ByVal senha As String,
                            ByVal ComputerName As ArrayList, ByVal Cod As String) As Boolean

        Dim sArquivos As ArrayList = New ArrayList
        Dim iContador As Integer
        Dim fwr As FtpWebRequest = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/PCON/")
        fwr.Credentials = New NetworkCredential(usuario, senha)
        fwr.Method = WebRequestMethods.Ftp.ListDirectory
        Dim sr As New StreamReader(fwr.GetResponse().GetResponseStream())
        Dim str As String = sr.ReadLine()
        Dim fluxoTexto As IO.StreamWriter
        Dim linhaTexto As String = ""
        Dim Cont As Boolean = False
        Dim CaminhoCop As String = System.Reflection.Assembly.GetExecutingAssembly().Location
        Dim NovaLinha As String = ""
        Dim ContTraco As Integer = 0
        Dim DadosCapturaIP As String = ""
        Dim DadosCapturaHost As String = ""
        Dim Virarchave As Integer = 0
        Dim CodigoUnico As String = ""
        Dim Agora As Date
        Dim Onlinetratado As String = ""
        Dim Compartivo As String = ""
        Dim LerArquivo As IO.StreamReader
        Dim PCONstring As String = ""

        CaminhoCop = CaminhoCop.Replace("Walle_Client.exe", "")

        If File.Exists(CaminhoCop & "\BIN.txt") = True Then

            LerArquivo = New IO.StreamReader(CaminhoCop & "\BIN.txt")
            linhaTexto = LerArquivo.ReadLine

            LerArquivo.Close()

            ComputerName.Item(0) = linhaTexto
            Frm_Principal.CodClienteUser = linhaTexto

            PCONstring = linhaTexto

        End If

        If PCONstring = "" Then

            PCONstring = "NOTHING"

        End If

        'Mandando informações que a maquina esta online
        iContador = 0
        While Not str Is Nothing

            NovaLinha = str
            str = sr.ReadLine()
            linhaTexto = NovaLinha.Replace(".txt", "")

            Onlinetratado = Mid(linhaTexto, linhaTexto.Length - 18, 19)
            Onlinetratado = linhaTexto.Replace(Onlinetratado, "")
            Onlinetratado = Mid(Onlinetratado, 3, Onlinetratado.Length)

            Compartivo = PCONstring & "_" & ComputerName.Item(2) & "_" & ComputerName.Item(1) & "_"

            If Onlinetratado = Compartivo Then

                sr.Close()
                sr = Nothing
                fwr = Nothing

                fwr = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/PCON/" & linhaTexto & ".txt")
                fwr.Credentials = New NetworkCredential(usuario, senha)
                fwr.Method = WebRequestMethods.Ftp.DeleteFile
                Dim ftpResp As FtpWebResponse = fwr.GetResponse

                Exit While

            End If

            linhaTexto = ""

        End While

        Try

            sr.Close()
            sr = Nothing
            fwr = Nothing

        Catch ex As Exception



        End Try

        fwr = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Ativas/User")
        fwr.Credentials = New NetworkCredential(usuario, senha)
        fwr.Method = WebRequestMethods.Ftp.ListDirectory
        sr = New StreamReader(fwr.GetResponse().GetResponseStream())
        str = sr.ReadLine()

        'Verificando se o exe já tem licenca ativa
        iContador = 0
        While Not str Is Nothing

            NovaLinha = str
            str = sr.ReadLine()
            linhaTexto = NovaLinha.Replace(".txt", "")

            If linhaTexto = ComputerName.Item(0) Then

                sr.Close()
                sr = Nothing
                fwr = Nothing

                RegistrarPCOnline(ComputerName, CaminhoCop, arquivoFTP, usuario, senha, Cod, PCONstring, 0)
                Return True
                Exit Function

            End If

            linhaTexto = ""

        End While

        sr.Close()
        sr = Nothing
        fwr = Nothing

        fwr = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Regoff")
        fwr.Credentials = New NetworkCredential(usuario, senha)
        fwr.Method = WebRequestMethods.Ftp.ListDirectory
        sr = New StreamReader(fwr.GetResponse().GetResponseStream())
        str = sr.ReadLine()

        'Procurando lincenca
        iContador = 0
        While Not str Is Nothing

            linhaTexto = str
            str = sr.ReadLine()

            For i As Integer = 0 To linhaTexto.Length - 1

                If linhaTexto.Chars(i) = "_" Then

                    Virarchave = Virarchave + 1

                Else

                    If Virarchave = 1 Then

                        DadosCapturaIP = DadosCapturaIP & linhaTexto.Chars(i)

                    ElseIf Virarchave = 2 Then

                        DadosCapturaHost = DadosCapturaHost & linhaTexto.Chars(i)

                    Else

                        CodigoUnico = CodigoUnico & linhaTexto.Chars(i)

                    End If

                End If

            Next

            DadosCapturaHost = DadosCapturaHost.Replace(".txt", "")

            If DadosCapturaHost = ComputerName.Item(1) And DadosCapturaIP = ComputerName.Item(2) Then

                NovaLinha = ""
                NovaLinha = System.Environment.UserName
                Agora = Now
                NovaLinha = NovaLinha & "-" & Agora.ToString("yyyy_MM_dd_HH_mm_ss") & "-"

                fluxoTexto = New IO.StreamWriter(CaminhoCop _
                                                 & "\" & NovaLinha & CodigoUnico & ".txt")
                fluxoTexto.Close()

                Cont = True

                Exit While

            End If

        End While

        sr.Close()
        sr = Nothing
        fwr = Nothing

        If Cont = True Then

            UploadFile(CaminhoCop & NovaLinha & CodigoUnico & ".txt",
                   arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Ativas/" & NovaLinha & ComputerName.Item(0) & ".txt",
                   usuario, senha)

            fluxoTexto = New IO.StreamWriter(CaminhoCop & CodigoUnico & ".txt")
            fluxoTexto.Close()

            UploadFile(CaminhoCop & CodigoUnico & ".txt",
                arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Ativas/User/" & CodigoUnico & ".txt",
                usuario, senha)

            My.Computer.FileSystem.DeleteFile(CaminhoCop & CodigoUnico & ".txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
            My.Computer.FileSystem.DeleteFile(CaminhoCop & NovaLinha & CodigoUnico & ".txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
            My.Computer.FileSystem.DeleteFile(CaminhoCop & "BIN.txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

            fwr = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Regoff/" & linhaTexto)
            fwr.Credentials = New NetworkCredential(usuario, senha)
            fwr.Method = WebRequestMethods.Ftp.DeleteFile
            Dim ftpResp As FtpWebResponse = fwr.GetResponse

            fluxoTexto = New IO.StreamWriter(CaminhoCop & "BIN.txt")
            fluxoTexto.WriteLine(CodigoUnico)
            fluxoTexto.Close()

            SetAttr(CaminhoCop & "BIN.txt", vbHidden)

            Frm_Principal.CodClienteUser = CodigoUnico

            Return True

        Else

            If Frm_Principal.LicencaOndemand = True Then

                fwr = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/Licenca")
                fwr.Credentials = New NetworkCredential(usuario, senha)
                fwr.Method = WebRequestMethods.Ftp.ListDirectory
                sr = New StreamReader(fwr.GetResponse().GetResponseStream())
                str = sr.ReadLine()

                'Procure se existe licenca disponivel
                iContador = 0
                While Not str Is Nothing

                    NovaLinha = str
                    linhaTexto = NovaLinha.Replace(".txt", "")

                    If NovaLinha <> linhaTexto Then

                        CodigoUnico = linhaTexto

                        NovaLinha = ""
                        NovaLinha = System.Environment.UserName
                        Agora = Now
                        NovaLinha = NovaLinha & "-" & Agora.ToString("yyyy_MM_dd_HH_mm_ss") & "-"

                        fluxoTexto = New IO.StreamWriter(CaminhoCop _
                                                     & "\" & NovaLinha & CodigoUnico & ".txt")
                        fluxoTexto.Close()

                        Frm_Principal.CodClienteUser = linhaTexto
                        Cont = True

                        sr.Close()
                        sr = Nothing
                        fwr = Nothing

                        fwr = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/" & str)
                        fwr.Credentials = New NetworkCredential(usuario, senha)
                        fwr.Method = WebRequestMethods.Ftp.DeleteFile
                        Dim ftpResp As FtpWebResponse = fwr.GetResponse

                        Exit While

                    End If

                    str = sr.ReadLine()

                End While

                Try

                    sr.Close()
                    sr = Nothing
                    fwr = Nothing

                Catch ex As Exception



                End Try

                If Cont = True Then

                    If File.Exists(CaminhoCop & "BIN.txt") = True Then

                        My.Computer.FileSystem.DeleteFile(CaminhoCop & "BIN.txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

                    End If

                    fluxoTexto = New IO.StreamWriter(CaminhoCop & "BIN.txt")
                    fluxoTexto.WriteLine(CodigoUnico)
                    fluxoTexto.Close()

                    SetAttr(CaminhoCop & "BIN.txt", vbHidden)

                    Thread.Sleep(1000)

                    UploadFile(CaminhoCop & NovaLinha & CodigoUnico & ".txt",
                   arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Ativas/" & NovaLinha & ComputerName.Item(0) & ".txt",
                   usuario, senha)

                    fluxoTexto = New IO.StreamWriter(CaminhoCop & CodigoUnico & ".txt")
                    fluxoTexto.Close()

                    Thread.Sleep(1000)

                    UploadFile(CaminhoCop & CodigoUnico & ".txt",
                        arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Ativas/User/" & CodigoUnico & ".txt",
                        usuario, senha)

                    fluxoTexto = New IO.StreamWriter(CaminhoCop & CodigoUnico & "_" & ComputerName.Item(2).ToString & "_" & ComputerName.Item(1).ToString & ".txt")
                    fluxoTexto.Close()

                    Thread.Sleep(1000)

                    UploadFile(CaminhoCop & CodigoUnico & "_" & ComputerName.Item(2).ToString & "_" & ComputerName.Item(1).ToString & ".txt",
                   arquivoFTP & "/Walle/Client/" & Cod & "/Licenca/Pendente/" _
                               & CodigoUnico & "_" & ComputerName.Item(2).ToString & "_" & ComputerName.Item(1).ToString & ".txt",
                   usuario, senha)

                    Thread.Sleep(1000)

                    If File.Exists(CaminhoCop & CodigoUnico & ".txt") = True Then

                        My.Computer.FileSystem.DeleteFile(CaminhoCop & CodigoUnico & ".txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

                    End If

                    If File.Exists(CaminhoCop & NovaLinha & CodigoUnico & ".txt") = True Then

                        My.Computer.FileSystem.DeleteFile(CaminhoCop & NovaLinha & CodigoUnico & ".txt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

                    End If

                    If File.Exists(CaminhoCop & CodigoUnico & "_" & ComputerName.Item(2).ToString & "_" & ComputerName.Item(1).ToString & ".txt") = True Then

                        My.Computer.FileSystem.DeleteFile(CaminhoCop & CodigoUnico & "_" & ComputerName.Item(2).ToString & "_" & ComputerName.Item(1).ToString & ".txt",
                                                      FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

                    End If

                    Return True

                Else

                    If PCONstring = "NOTHING" Then

                        RegistrarPCOnline(ComputerName, CaminhoCop, arquivoFTP, usuario, senha, Cod, PCONstring, 2)

                    Else

                        RegistrarPCOnline(ComputerName, CaminhoCop, arquivoFTP, usuario, senha, Cod, PCONstring, 1)

                    End If

                    Return False

                End If

            Else

                If PCONstring = "NOTHING" Then

                    RegistrarPCOnline(ComputerName, CaminhoCop, arquivoFTP, usuario, senha, Cod, PCONstring, 2)

                Else

                    RegistrarPCOnline(ComputerName, CaminhoCop, arquivoFTP, usuario, senha, Cod, PCONstring, 1)

                End If

                Return False

            End If

        End If

    End Function

    Public Function Ondemand(ByVal arquivoFTP As String,
                            ByVal usuario As String, ByVal senha As String,
                            ByVal ComputerName As ArrayList, ByVal Cod As String)

        Dim fwr As FtpWebRequest = FtpWebRequest.Create(arquivoFTP & "/Walle/Client/" & Cod)
        fwr.Credentials = New NetworkCredential(usuario, senha)
        fwr.Method = WebRequestMethods.Ftp.ListDirectory
        Dim sr As New StreamReader(fwr.GetResponse().GetResponseStream())
        Dim str As String = sr.ReadLine()
        Dim linhaTexto As String = ""
        Dim Cont As Boolean = False
        Dim CaminhoCop As String = System.Reflection.Assembly.GetExecutingAssembly().Location
        Dim NovaLinha As String = ""

        While Not str Is Nothing

            NovaLinha = str
            str = sr.ReadLine()
            linhaTexto = NovaLinha.Replace(".txt", "")

            If linhaTexto = "ondemand" Then

                Cont = True

            End If

        End While

        sr.Close()
        sr = Nothing
        fwr = Nothing

        Return Cont

    End Function

    Private Sub RegistrarPCOnline(ByVal ComputerName As ArrayList, ByVal CaminhoCop As String, ByVal arquivoFTP As String,
                                  ByVal usuario As String, ByVal senha As String, ByVal Cod As String, ByVal Ativa As String, ByVal Tipo As Integer)

        Dim DataColeta As Date
        Dim Compartivo As String
        Dim fluxotexto As IO.StreamWriter

        '########################################################################################################################
        'Cria o arquivo de computador ativo
        DataColeta = Now
        Compartivo = Tipo & "_" & Ativa & "_" & ComputerName.Item(2) & "_" & ComputerName.Item(1) & "_"

        fluxotexto = New IO.StreamWriter(CaminhoCop & Compartivo & DataColeta.ToString("yyyy-MM-dd-HH-mm-ss") & ".txt")
        fluxotexto.Close()

        UploadFile(CaminhoCop & Compartivo & DataColeta.ToString("yyyy-MM-dd-HH-mm-ss") & ".txt",
                   arquivoFTP & "/Walle/Client/" & Cod & "/PCON/" & Compartivo & DataColeta.ToString("yyyy-MM-dd-HH-mm-ss") & ".txt",
                   usuario, senha)

        My.Computer.FileSystem.DeleteFile(CaminhoCop & "\" & Compartivo & DataColeta.ToString("yyyy-MM-dd-HH-mm-ss") & ".txt",
                                          FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)

        'FIM
        '########################################################################################################################

    End Sub

    Public Sub UploadFile(ByVal _FileName As String, ByVal _UploadPath As String, ByVal _FTPUser As String, ByVal _FTPPass As String)
        Dim _FileInfo As New System.IO.FileInfo(_FileName)

        ' Create FtpWebRequest object from the Uri provided
        Dim _FtpWebRequest As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(New Uri(_UploadPath)), System.Net.FtpWebRequest)

        ' Provide the WebPermission Credintials
        _FtpWebRequest.Credentials = New System.Net.NetworkCredential(_FTPUser, _FTPPass)

        ' By default KeepAlive is true, where the control connection is not closed
        ' after a command is executed.
        _FtpWebRequest.KeepAlive = False

        ' set timeout for 20 seconds
        _FtpWebRequest.Timeout = 20000

        ' Specify the command to be executed.
        _FtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile

        ' Specify the data transfer type.
        _FtpWebRequest.UseBinary = True

        ' Notify the server about the size of the uploaded file
        _FtpWebRequest.ContentLength = _FileInfo.Length

        ' The buffer size is set to 2kb
        Dim buffLength As Integer = 2048
        Dim buff(buffLength - 1) As Byte

        ' Opens a file stream (System.IO.FileStream) to read the file to be uploaded
        Dim _FileStream As System.IO.FileStream = _FileInfo.OpenRead()

        ' Stream to which the file to be upload is written
        Dim _Stream As System.IO.Stream = _FtpWebRequest.GetRequestStream()

        ' Read from the file stream 2kb at a time
        Dim contentLen As Integer = _FileStream.Read(buff, 0, buffLength)

        ' Till Stream content ends
        Do While contentLen <> 0
            ' Write Content from the file stream to the FTP Upload Stream
            _Stream.Write(buff, 0, contentLen)
            contentLen = _FileStream.Read(buff, 0, buffLength)
        Loop

        ' Close the file stream and the Request Stream
        _Stream.Close()
        _Stream.Dispose()
        _FileStream.Close()
        _FileStream.Dispose()
    End Sub

    Public Function GetUserClient() As String

        Dim chave As New Rfc2898DeriveBytes("yekruof", sal)
        Dim algoritmo = New RijndaelManaged()
        Dim fluxoTexto As IO.StreamReader
        Dim linhaTexto As String
        Dim Cliente As String = ""
        Dim USERCLIENT As String = System.Reflection.Assembly.GetExecutingAssembly().Location & "USERCLIENT.txt"

        USERCLIENT = USERCLIENT.Replace("Walle_Client.exe", "")

        If IO.File.Exists(USERCLIENT) Then

            fluxoTexto = New IO.StreamReader(USERCLIENT)
            linhaTexto = fluxoTexto.ReadLine

            fluxoTexto.Close()

            textoCifrado = Convert.FromBase64String(linhaTexto)

            algoritmo.Key = chave.GetBytes(16)
            algoritmo.IV = chave.GetBytes(16)

            Using StreamFonte = New MemoryStream(textoCifrado)

                Using StreamDestino As New MemoryStream()

                    Using crypto As New CryptoStream(StreamFonte, algoritmo.CreateDecryptor(), CryptoStreamMode.Read)

                        moveBytes(crypto, StreamDestino)

                        Dim bytesDescriptografados() As Byte = StreamDestino.ToArray()
                        Dim mensagemDescriptografada = New UnicodeEncoding().GetString(bytesDescriptografados)

                        Cliente = mensagemDescriptografada

                    End Using

                End Using

            End Using

        Else

            MsgBox("Walle: Atenção alguns arquivos foram movidos ou modificados indevidamente, por favor entre em contato com o fornecedor.", vbExclamation)

            Application.Exit()

        End If

        Frm_Principal.CodClienteWalle = Cliente

        Return Cliente & "/key.txt"

    End Function

    Public Function Criptografar(ByVal TextoCif As String, ByVal SenhaCif As String) As String

        senha = SenhaCif
        mensagem = TextoCif

        Dim chave As New Rfc2898DeriveBytes(senha, sal)
        ' criptografa os dados
        Dim algoritmo = New RijndaelManaged()
        algoritmo.Key = chave.GetBytes(16)
        algoritmo.IV = chave.GetBytes(16)

        Dim fonteBytes() As Byte = New System.Text.UnicodeEncoding().GetBytes(mensagem)

        Using StreamFonte = New MemoryStream(fonteBytes)
            Using StreamDestino As New MemoryStream()
                Using crypto As New CryptoStream(StreamFonte, algoritmo.CreateEncryptor(), CryptoStreamMode.Read)
                    moveBytes(crypto, StreamDestino)
                    textoCifrado = StreamDestino.ToArray()
                End Using
            End Using
        End Using

        Return Convert.ToBase64String(textoCifrado)

    End Function

    Public Function GetLocationPath() As String

        Dim LocationString As String = System.Reflection.Assembly.GetExecutingAssembly().Location & "LOCATION.txt"
        Dim fluxoTexto As IO.StreamReader
        Dim linhaTexto As String = ""

        LocationString = LocationString.Replace("Walle_Client.exe", "")

        If IO.File.Exists(LocationString) Then

            fluxoTexto = New IO.StreamReader(LocationString)
            linhaTexto = fluxoTexto.ReadLine
            fluxoTexto.Close()

        End If

        Return linhaTexto

    End Function

    Public Function Check_Licenca(ByVal Server As String, ByVal Arquivo As String,
                                     ByVal Usuario As String, ByVal Senha As String, ByVal ClienteCod As String)
        Try
            ' I set the Ip address of the remote computer.
            Dim IpAddress As String = Server

            ClienteCod = ClienteCod.Replace("/key.txt", "")

            ' I set the directory name of the remote computer
            Dim FolderName As String = “Walle/Client/” & ClienteCod & "/Licenca/Ativas/User"

            ' I set the file name of the remote computer
            Dim FileName As String = Arquivo

            ' I set the absolute file name of the remote computer
            Dim PathAndFileNameToCheck As String =
                IpAddress & “/” & FolderName & “/” & FileName

            ' I set the credentials of the remote computer.
            Dim User_Name As String = Usuario
            Dim User_Password As String = Senha

            ' I set the Ftp object.
            Dim FtpWebRequest As System.Net.FtpWebRequest =
                DirectCast(System.Net.WebRequest.Create(PathAndFileNameToCheck), System.Net.FtpWebRequest)
            FtpWebRequest.Credentials = New System.Net.NetworkCredential(User_Name, User_Password)
            FtpWebRequest.Proxy = Nothing
            FtpWebRequest.GetResponse()

            Return True

        Catch ex As Exception
            ' I show an error message if the sub generates an error.
            If ex.Message.IndexOf(“550”) > 0 Then
                Return False
            Else
                Return False
            End If
        End Try
    End Function


    Public Function GetChromeActiveWindowUrl() As [String]
        Dim procs = Process.GetProcessesByName(Frm_Principal.ChromeProcess2)

        If (procs.Length = 0) Then
            Return [String].Empty
        End If

        Return procs _
    .Where(Function(p) p.MainWindowHandle <> IntPtr.Zero) _
    .Select(Function(s) GetUrlControl(s)) _
    .Where(Function(p) p IsNot Nothing) _
    .Select(Function(s) GetValuePattern(s)) _
    .Where(Function(p) p.Item2.Length > 0) _
    .Select(Function(s) GetValuePatternUrl(s)) _
    .FirstOrDefault

    End Function

    Private Function GetUrlControl(
    proses As Process) _
    As AutomationElement

        Dim propCondition =
        New PropertyCondition(
        AutomationElement.NameProperty,
        AddressCtl)
        Return AutomationElement _
        .FromHandle(proses.MainWindowHandle) _
        .FindFirst(
            TreeScope.Descendants,
            propCondition)

    End Function

    Private Function GetValuePatternUrl(
    element As Tuple(Of
    AutomationElement, AutomationPattern())) As [String]

        Dim ap = element.Item2(0)
        Dim ovp = element.Item1.GetCurrentPattern(ap)
        Dim vp = CType(ovp, ValuePattern)

        Return vp.Current.Value
    End Function

    Private Function GetValuePattern(
    element As AutomationElement) _
As Tuple(Of
          AutomationElement,
          AutomationPattern())

        Return New Tuple(Of
          AutomationElement,
          AutomationPattern())(
          element,
          element.GetSupportedPatterns())
    End Function

    Public Function ObtemEnderecoIP() As String

        Dim IPList As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName)
        Dim IP As IPAddress
        For Each IP In IPList.AddressList
            'Only return IPv4 routable IPs
            If (IP.AddressFamily = Sockets.AddressFamily.InterNetwork) Then
                Return IP.ToString
            End If
        Next

        Return ""

    End Function

    Public Sub GravarRegistro(ByVal Location As String)

        Dim car As New List(Of Class_Processos)

        Dim fs As New FileStream("test_serialize.dat", FileMode.Create)

        Dim bf As New BinaryFormatter

        bf.Serialize(fs, car)

        fs.Close()


    End Sub

End Class
