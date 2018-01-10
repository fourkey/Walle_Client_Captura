Public Class Class_Processos

    Private _iID As String
    Public Property ID() As String
        Get
            Return _iID
        End Get
        Set(ByVal value As String)
            _iID = value
        End Set
    End Property

    Private _Nome As String
    Public Property Nome() As String
        Get
            Return _Nome
        End Get
        Set(ByVal value As String)
            _Nome = value
        End Set
    End Property


    Private _Processo As String
    Public Property Processo() As String
        Get
            Return _Processo
        End Get
        Set(ByVal value As String)
            _Processo = value
        End Set
    End Property

    Private _Local As String
    Public Property Local() As String
        Get
            Return _Local
        End Get
        Set(ByVal value As String)
            _Local = value
        End Set
    End Property

    Private _HoraIni As String
    Public Property HoraIni() As String
        Get
            Return _HoraIni
        End Get
        Set(ByVal value As String)
            _HoraIni = value
        End Set
    End Property

    Private _HoraFim As String
    Public Property HoraFim() As String
        Get
            Return _HoraFim
        End Get
        Set(ByVal value As String)
            _HoraFim = value
        End Set
    End Property

    Private _Tempo As String
    Public Property Tempo() As String
        Get
            Return _Tempo
        End Get
        Set(ByVal value As String)
            _Tempo = value
        End Set
    End Property

    Private _Chave As String
    Public Property Chave() As String
        Get
            Return _Chave
        End Get
        Set(ByVal value As String)
            _Chave = value
        End Set
    End Property

    Private _URL As String
    Public Property URL() As String
        Get
            Return _URL
        End Get
        Set(ByVal value As String)
            _URL = value
        End Set
    End Property

    Private _ProcessoCompleto As Process
    Public Property ProcessoCompleto() As Process
        Get
            Return _ProcessoCompleto
        End Get
        Set(ByVal value As Process)
            _ProcessoCompleto = value
        End Set
    End Property

    Private _Versao As String
    Public Property Versao() As String
        Get
            Return _Versao
        End Get
        Set(ByVal value As String)
            _Versao = value
        End Set
    End Property

    Private _Rastrear As String
    Public Property Rasterar() As String
        Get
            Return _Rastrear
        End Get
        Set(ByVal value As String)
            _Rastrear = value
        End Set
    End Property

End Class
