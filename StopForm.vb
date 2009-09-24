Public Class StopForm
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Friend WithEvents MousePictureBox As System.Windows.Forms.PictureBox
    Friend WithEvents StartPauseButton As System.Windows.Forms.Button

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(StopForm))
        Me.StartPauseButton = New System.Windows.Forms.Button
        Me.MousePictureBox = New System.Windows.Forms.PictureBox
        CType(Me.MousePictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'StartPauseButton
        '
        Me.StartPauseButton.Location = New System.Drawing.Point(68, 153)
        Me.StartPauseButton.Name = "StartPauseButton"
        Me.StartPauseButton.Size = New System.Drawing.Size(75, 23)
        Me.StartPauseButton.TabIndex = 0
        Me.StartPauseButton.Text = "Start"
        '
        'MousePictureBox
        '
        Me.MousePictureBox.Image = Global.Move_Mouse.My.Resources.Resources.optical_mouse_128x128
        Me.MousePictureBox.Location = New System.Drawing.Point(38, 12)
        Me.MousePictureBox.Name = "MousePictureBox"
        Me.MousePictureBox.Size = New System.Drawing.Size(135, 135)
        Me.MousePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.MousePictureBox.TabIndex = 1
        Me.MousePictureBox.TabStop = False
        '
        'StopForm
        '
        Me.AcceptButton = Me.StartPauseButton
        Me.AutoScaleBaseSize = New System.Drawing.Size(6, 16)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.ClientSize = New System.Drawing.Size(210, 188)
        Me.Controls.Add(Me.MousePictureBox)
        Me.Controls.Add(Me.StartPauseButton)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(136, 80)
        Me.Name = "StopForm"
        Me.Opacity = 0.75
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Move Mouse"
        Me.TopMost = True
        CType(Me.MousePictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Const MOUSEEVENTF_LEFTDOWN As Int32 = &H2
    Const MOUSEEVENTF_LEFTUP As Int32 = &H4
    Const MOUSEEVENTF_MIDDLEDOWN As Int32 = &H20
    Const MOUSEEVENTF_MIDDLEUP As Int32 = &H40
    Const MOUSEEVENTF_MOVE As Int32 = &H1
    Const MOUSEEVENTF_ABSOLUTE As Int32 = &H8000
    Const MOUSEEVENTF_RIGHTDOWN As Int32 = &H8
    Const MOUSEEVENTF_RIGHTUP As Int32 = &H10

    Private Declare Sub mouse_event Lib "user32.dll" ( _
        ByVal dwFlags As Int32, _
        ByVal dx As Int32, _
        ByVal dy As Int32, _
        ByVal cButtons As Int32, _
        ByVal dwExtraInfo As Int32)

    Dim moveMouseThread As Threading.Thread

    Private Sub StopForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        moveMouseThread.Abort()
    End Sub

    Private Sub StopForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        StartPauseButton.PerformClick()
    End Sub

    Private Sub StartPauseButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StartPauseButton.Click
        If StartPauseButton.Text = "Start" Then
            moveMouseThread = New Threading.Thread(AddressOf MoveMouse)
            moveMouseThread.Start()
            StartPauseButton.Text = "Pause"
            Me.Opacity = 0.75
        Else
            moveMouseThread.Abort()
            StartPauseButton.Text = "Start"
            Me.Opacity = 1
            Me.WindowState = FormWindowState.Minimized
        End If
    End Sub

    Sub MoveMouse()
        Do
            System.Threading.Thread.Sleep(30000)

            Dim clickMousePos As Point = Cursor.Position

            Call mouse_event(MOUSEEVENTF_LEFTDOWN Or MOUSEEVENTF_LEFTUP, 0&, 0&, 0&, 0&)

            Cursor.Position = New Point(Me.Left, Me.Top)

            For loopCount As Integer = 0 To Me.Width
                Cursor.Position = New Point(Cursor.Position.X + 1, Cursor.Position.Y)
                System.Threading.Thread.Sleep(5)
            Next

            For loopCount As Integer = 0 To Me.Height
                Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 1)
                System.Threading.Thread.Sleep(5)
            Next

            For loopCount As Integer = 0 To Me.Width
                Cursor.Position = New Point(Cursor.Position.X - 1, Cursor.Position.Y)
                System.Threading.Thread.Sleep(5)
            Next

            For loopCount As Integer = 0 To Me.Height
                Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 1)
                System.Threading.Thread.Sleep(5)
            Next

            Cursor.Position = clickMousePos
        Loop
    End Sub
End Class
