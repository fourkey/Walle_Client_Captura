<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Frm_Principal
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Frm_Principal))
        Me.Timer_ColetaDados = New System.Windows.Forms.Timer(Me.components)
        Me.Timer_ColetaTeclas = New System.Windows.Forms.Timer(Me.components)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.Timer_ColetaClique = New System.Windows.Forms.Timer(Me.components)
        Me.t_Verificar_Licenca = New System.Windows.Forms.Timer(Me.components)
        Me.Timer_Licenca = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'Timer_ColetaDados
        '
        Me.Timer_ColetaDados.Interval = 1000
        '
        'Timer_ColetaTeclas
        '
        Me.Timer_ColetaTeclas.Enabled = True
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(1, 4)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(411, 91)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Loading..."
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'NotifyIcon1
        '
        Me.NotifyIcon1.Text = "NotifyIcon1"
        Me.NotifyIcon1.Visible = True
        '
        'Timer_ColetaClique
        '
        '
        'Timer_Licenca
        '
        Me.Timer_Licenca.Interval = 300000
        '
        'Frm_Principal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(413, 97)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "Frm_Principal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Frm_Principal"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Timer_ColetaDados As System.Windows.Forms.Timer
    Friend WithEvents Timer_ColetaTeclas As System.Windows.Forms.Timer
    Friend WithEvents Label1 As Label
    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents Timer_ColetaClique As Timer
    Friend WithEvents t_Verificar_Licenca As Timer
    Friend WithEvents Timer_Licenca As Timer
End Class
