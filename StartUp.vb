Module StartUp
    Sub Main()
        Application.EnableVisualStyles()
        Application.DoEvents()

        Dim stopForm As New StopForm

        stopForm.ShowDialog()
    End Sub
End Module
