Imports System.Threading
Imports System.Management
Imports System.IO
Imports System.Net.NetworkInformation

Public Class Frm_Principal


    Public Versao As String = "4.5.1"
    Public Lista As New List(Of Class_Processos) 'Lista de processos executados
    Public ListaFinal As New List(Of Class_Processos) 'Lista que é enviada pela Thread para gerar o arquivo csv
    Public DadosAtuais As New Class_Processos 'Dados atuais que estão em execução em primeiro palno
    Public MouseX As Integer 'Posição X do mouse
    Public MouseY As Integer 'Posição Y do mouse
    Public TempoMouseOcioso As Integer 'Tempo Ociso sem mexer o mouse
    Public HoraMouseOciso As Date = Format(Now, "yyyy-MM-dd HH:mm:ss") 'Hora do ultimo envio do csv
    Public Letra As String 'Tecla capturada
    Public HoraAcionamentoTeclado As Date = Format(Now, "yyyy-MM-dd HH:mm:ss") 'Hora do ultimo acionamento do teclado
    Public ChaveDeOciosidade As Boolean = False
    Public ContruirOciosidade As Boolean = False
    Public Clep As Boolean = False
    Public ContTecla As Integer 'Controla as teclas Ctrl + Alt + F
    Public Rastreio As String = "" 'Variavel que controla aonde o arquivo passou
    Public UltimaHoraCapturada As String 'Garante que o intervalo de horas não tenha margem de erro
    Private WithEvents BackgroundWorker1 As New System.ComponentModel.BackgroundWorker
    Private Declare Function GetAsyncKeyState Lib "User32" (ByVal vKey As Integer) As Integer 'Detecta o clique do mouse
    Dim MBE, MBM, MBD As Integer 'Botoes

    Dim Funcao As New Funcoes_Sistema 'Classe de funcoes do sistema
    Dim HoraEnvio As Date = Format(Now, "yyyy-MM-dd HH:mm:ss") 'Hora do ultimo envio do csv
    Dim HoraEnvioFim As Date = Format(Now, "yyyy-MM-dd HH:mm:ss") 'Hora atual para verificar os 60 segundos do envio do csv
    Dim Segundos As Integer 'Diferença entre as duas variaveis acima
    Dim ContruirArquivo As New Thread(AddressOf Funcao.GerarArquivo) 'Thread de disparo do csv
    Public MeuArray As New ArrayList 'Contem 3 informações de identificacao da maquina

    Public fluxoTextoTem As IO.StreamWriter

    Dim Pub As New Util

    'CRIPTOGRAFIA
    Public Shared UserCript As String
    Public Shared PassCript As String
    Public Shared CaminhoFtp As String
    Public Shared ClientFourkey As String = ""
    Public Shared ClientLocation As String = ""
    Public Shared ClientLocalPasta As String = ""
    Public Shared EnderecoIPProcessador As String = ""
    Public Shared CodClienteWalle As String = ""
    Public Shared CodClienteUser As String = ""
    Public Shared LicencaOndemand As Boolean


    'URL
    Public Const ChromeProcess2 As [String] = "chrome"
    Public Const AddressCtl2 As [String] = "Address and search bar"

    Private Sub Frm_Principal_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim Cont As Integer = 0
        Dim ContService As Integer = 0
        Dim strProcessorId As String = String.Empty
        Dim query As New SelectQuery("Win32_processor")
        Dim search As New ManagementObjectSearcher(query)
        Dim info As ManagementObject
        Dim fluxoTexto As IO.StreamWriter
        Dim HostnameClient As String
        Dim IPClient As String

        Me.ShowInTaskbar = False
        Me.WindowState = FormWindowState.Minimized



        UserCript = Pub.Decifra(My.Settings.CriptUser)
        PassCript = Pub.Decifra(My.Settings.CriptPass)
        CaminhoFtp = Pub.Decifra(My.Settings.PathFtp)

        Pub.Escreve_Log("DEBUG - Dados do FTP: " & vbCrLf &
                        "Usuario: " & UserCript & " - Caminho: " & CaminhoFtp)

        Application.DoEvents()

        'Localiza a pasta de instalação
        ClientLocalPasta = System.Reflection.Assembly.GetExecutingAssembly().Location
        ClientLocalPasta = ClientLocalPasta.Replace("Walle_Client.exe", "")

        Pub.Escreve_Log("DEBUG - Path EXE: " & ClientLocalPasta)

        'Buscar endereco Processador
        For Each info In search.Get()

            strProcessorId = info("processorId").ToString()

        Next

        strProcessorId = strProcessorId.Replace(":", "-")
        strProcessorId = strProcessorId.Replace(".", "-")
        strProcessorId = strProcessorId.Replace("{", "")
        strProcessorId = strProcessorId.Replace("}", "")

        EnderecoIPProcessador = strProcessorId

        HostnameClient = Environment.MachineName
        IPClient = Funcao.ObtemEnderecoIP()

        MeuArray.Add("")
        MeuArray.Add(HostnameClient)
        MeuArray.Add(IPClient)

        For Each processo As Process In Process.GetProcesses()

            If processo.ProcessName = "Walle_Client" Then

                Cont = Cont + 1

            End If

            If processo.ProcessName = "Walle_Client_Data" Then

                ContService = 1

            End If

        Next processo

        If Cont > 1 Then

            Application.Exit()

        End If

        If ContService = 0 Then

            Try

                Shell("Walle_Client_Data.exe")

            Catch ex As Exception

                'Nada a fazer

            End Try

        End If

        Try

            'Localiza a pasta onde deve ser salvo os pacotes
            ClientLocation = Funcao.GetLocationPath()

            Pub.Escreve_Log("DEBUG - Path Pacotes: " & ClientLocation)

            If Directory.Exists(ClientLocation & "\Address") = False Then

                Directory.CreateDirectory(ClientLocation & "\Address")

            End If

            SetAttr(ClientLocation & "\Address", vbHidden)

            'Recebe o código de descriptografia
            If Pub.VerificaConexaoFtp() = True Then

                ClientFourkey = Funcao.descarregarArquivo(CaminhoFtp, UserCript, PassCript, Funcao.GetUserClient())
                LicencaOndemand = Funcao.Ondemand(CaminhoFtp, UserCript,
                                                         PassCript, MeuArray, CodClienteWalle)

                Try

                    If Funcao.descarregarArquivo2(CaminhoFtp, UserCript,
                                             PassCript, MeuArray, CodClienteWalle) = False Then

                        Timer_ColetaDados.Enabled = False
                        Timer_Licenca.Enabled = True
                        MeuArray.Item(0) = CodClienteUser

                    Else

                        Timer_ColetaDados.Enabled = True

                    End If

                Catch ex As Exception

                    Pub.Escreve_Log("CATCH - (" & MeuArray.Item(1) & " - " & MeuArray.Item(2) & ")" & ex.Message)

                    Timer_ColetaDados.Enabled = False
                    Timer_Licenca.Enabled = True

                End Try

            Else

                Pub.Escreve_Log("WARNING - (Load - Form_Principal) Sem conexao com o FTP para buscar os dados necessários.")

            End If


        Catch ex As Exception

            Pub.Escreve_Log("CATCH - (" & MeuArray.Item(1) & " - " & MeuArray.Item(2) & ")" & ex.Message)
            Application.Exit()

        End Try

        'Try

        '    If Funcao.Check_Licenca(CaminhoFtp, EnderecoIPProcessador & ".txt", UserCript, PassCript, Funcao.GetUserClient()) = False Then

        '        For Each processo As Process In Process.GetProcesses()

        '            If processo.ProcessName = "Walle_Client_Data" Then

        '                processo.Kill()

        '            End If

        '        Next processo

        '        MsgBox("Walle: Infelizmente sua licença expirou, por favor entre em contato com administrador do sistema. Código: " & EnderecoIPProcessador, vbInformation)

        '        Frm_Auxiliar.Fechamento = True

        '        Application.Exit()

        '    End If

        'Catch ex As Exception

        '    fluxoTexto = New IO.StreamWriter(ClientLocalPasta & "\Logs\Walle_Client-" _
        '                                             & Format(Now, "yyyy-MM-dd-HH-mm-ss") & ".txt")

        '    fluxoTexto.WriteLine(ex.Message)
        '    fluxoTexto.Close()

        '    Application.Exit()

        'End Try

        GC.Collect()

        'Frm_Analise.Show()

        'ContruirArquivo = New Thread(AddressOf Funcao.GerarArquivo)
        'ContruirArquivo.Start(ListaFinal)

    End Sub

    Private Sub Panel1_Click(sender As Object, e As EventArgs)

        Frm_Analise.Show()
        Me.Hide()

    End Sub

    Private Sub Timer_ColetaDados_Tick(sender As Object, e As EventArgs) Handles Timer_ColetaDados.Tick

        HoraEnvioFim = Format(Now, "yyyy-MM-dd HH:mm:ss")
        Segundos = DateDiff(DateInterval.Second, CDate(HoraEnvio),
                                                  CDate(HoraEnvioFim)).ToString

        If Segundos >= 300 Then

            Dim HoraFinal As Date = Format(Now, "yyyy-MM-dd HH:mm:ss")
            Dim Pacote3Informacoes As New Class_Processos

            DadosAtuais.HoraFim = HoraFinal
            UltimaHoraCapturada = DadosAtuais.HoraFim

            Try

                DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(DadosAtuais.HoraIni),
                                                    CDate(DadosAtuais.HoraFim)).ToString

            Catch ex As Exception

                If Lista.Count <> 0 Then

                    DadosAtuais.ID = 0
                    DadosAtuais.Nome = "Ociosidade"
                    DadosAtuais.Local = "Ociosidade"
                    DadosAtuais.Processo = "Ociosidade"
                    DadosAtuais.Chave = DadosAtuais.ID & "-" & DadosAtuais.Processo & "-" & DadosAtuais.Nome
                    DadosAtuais.HoraIni = Lista.Item(Lista.Count - 1).HoraFim

                    DadosAtuais.Tempo = DateDiff(DateInterval.Second, CDate(DadosAtuais.HoraIni),
                                                        CDate(DadosAtuais.HoraFim)).ToString

                Else

                    DadosAtuais.ID = 0

                End If

            End Try


            If DadosAtuais.Processo = "" Then

                DadosAtuais.ID = 0
                DadosAtuais.Nome = "Ociosidade"
                DadosAtuais.Local = "Ociosidade"
                DadosAtuais.Processo = "Ociosidade"
                DadosAtuais.Chave = DadosAtuais.ID & "-" & DadosAtuais.Processo & "-" & DadosAtuais.Nome

            End If

            If Rastreio <> "" And DadosAtuais.Rasterar = "" Then

                DadosAtuais.Rasterar = Rastreio
                Rastreio = ""

            End If

            Lista.Add(DadosAtuais)

            DadosAtuais = New Class_Processos

            DadosAtuais.HoraIni = Format(Now, "yyyy-MM-dd HH:mm:ss")

            DadosAtuais.Chave = "Colete"

            Clep = True 'Diz ao coletor de telas que ele finalizou automaticamente o pacote e não precisa iniciar a contagem pois a mesma foi iniciada acima

            HoraEnvio = HoraEnvioFim

            ListaFinal = New List(Of Class_Processos)

            ListaFinal = Lista

            Lista = New List(Of Class_Processos)

            ContruirArquivo = New Thread(AddressOf Funcao.GerarArquivo)
            ContruirArquivo.Start(ListaFinal)

            GC.Collect()

        End If

        Funcao.GetForgroundWindowInfo()

    End Sub

    Private Sub Timer_ColetaTeclas_Tick(sender As Object, e As EventArgs) Handles Timer_ColetaTeclas.Tick

        Try

            BackgroundWorker1.RunWorkerAsync()

        Catch ex As Exception

            Exit Sub

        End Try

    End Sub

    Private Sub BackgroundWorker1_DoWork(
    ByVal sender As Object,
    ByVal e As System.ComponentModel.DoWorkEventArgs
    ) Handles BackgroundWorker1.DoWork

        Try

            e.Result = Funcao.AcionamentoDoTeclado("")

        Catch ex As Exception

            Exit Sub

        End Try

    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(
    ByVal sender As Object,
    ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs
    ) Handles BackgroundWorker1.RunWorkerCompleted

        Try

            If e.Result.ToString <> "" Then

                HoraAcionamentoTeclado = CDate(e.Result.ToString)

            End If

        Catch ex As Exception

            Exit Sub

        End Try

    End Sub

    Private Sub Timer_ColetaClique_Tick(sender As Object, e As EventArgs) Handles Timer_ColetaClique.Tick

        Dim ThreadMouse As New Thread(AddressOf Funcao.GerarArquivo)

    End Sub

    Private Sub Timer_Licenca_Tick(sender As Object, e As EventArgs) Handles Timer_Licenca.Tick

        Dim fluxoTexto As IO.StreamWriter

        If Pub.VerificaConexaoFtp() = True Then

            Try

                If Funcao.descarregarArquivo2(CaminhoFtp, UserCript,
                                         PassCript, MeuArray, CodClienteWalle) = False Then

                    Timer_ColetaDados.Enabled = False

                Else

                    Timer_ColetaDados.Enabled = True
                    Timer_Licenca.Enabled = False

                End If

            Catch ex As Exception

                Pub.Escreve_Log("CATCH - (" & MeuArray.Item(1) & " - " & MeuArray.Item(2) & ")" & ex.Message)
                Application.Exit()

            End Try

        Else


            Pub.Escreve_Log("WARNING - (Timer_Licenca.Tick) - Falha ao se conectar com o Ftp para baixar os arquivos necessários.")
            Application.Exit()

        End If


    End Sub

    Private Sub Frm_Principal_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        If Me.WindowState = FormWindowState.Minimized Then

            Me.Hide()

        End If

    End Sub

    Private Sub NotifyIcon1_DoubleClick(sender As Object, e As EventArgs)

        'Frm_Auxiliar.Show()
        'Frm_Auxiliar.WindowState = FormWindowState.Normal
        'Frm_Auxiliar.ShowInTaskbar = True

        ' MsgBox("Walle: Atenção, você não tem permissão para abrir esta aplicação", vbInformation)

    End Sub


End Class