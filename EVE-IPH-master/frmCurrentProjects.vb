Public Class frmCurrentProjects
    Inherits Form

    Private Projects As List(Of CurrentProject)
    Private WithEvents lstProjects As New ListBox
    Private tabs As New TabControl
    Private lstItems As New ListView
    Private lstStages As New ListView
    Private lstBuild As New ListView
    Private lstBuy As New ListView
    Private WithEvents btnDeleteProject As New Button
    Private WithEvents btnDeleteRow As New Button
    Private WithEvents btnClose As New Button

    Public Sub New()
        Me.Text = "Текущие проекты"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1160, 660)
        Me.MinimizeBox = False

        lstProjects.Location = New Point(10, 10)
        lstProjects.Size = New Size(250, 560)
        Me.Controls.Add(lstProjects)

        tabs.Location = New Point(270, 10)
        tabs.Size = New Size(860, 560)
        Me.Controls.Add(tabs)

        SetupList(lstItems)
        SetupStagesList()
        SetupList(lstBuild, True)
        SetupList(lstBuy)

        Dim tabItems As New TabPage("Итог")
        Dim tabStages As New TabPage("Этапы")
        Dim tabBuild As New TabPage("Построить")
        Dim tabBuy As New TabPage("Купить")
        tabItems.Controls.Add(lstItems)
        tabStages.Controls.Add(lstStages)
        tabBuild.Controls.Add(lstBuild)
        tabBuy.Controls.Add(lstBuy)
        tabs.TabPages.Add(tabItems)
        tabs.TabPages.Add(tabStages)
        tabs.TabPages.Add(tabBuild)
        tabs.TabPages.Add(tabBuy)

        btnDeleteProject.Location = New Point(10, 580)
        btnDeleteProject.Size = New Size(120, 30)
        btnDeleteProject.Text = "Удалить проект"
        Me.Controls.Add(btnDeleteProject)

        btnDeleteRow.Location = New Point(270, 580)
        btnDeleteRow.Size = New Size(170, 30)
        btnDeleteRow.Text = "Удалить выбранное"
        Me.Controls.Add(btnDeleteRow)

        btnClose.Location = New Point(1010, 580)
        btnClose.Size = New Size(120, 30)
        btnClose.Text = "Закрыть"
        Me.Controls.Add(btnClose)

        LoadProjectList()
    End Sub

    Private Sub SetupList(List As ListView, Optional IncludeBuildColumns As Boolean = False)
        List.Dock = DockStyle.Fill
        List.View = View.Details
        List.FullRowSelect = True
        List.GridLines = True
        List.HideSelection = False
        List.Columns.Add("TypeID", 0, HorizontalAlignment.Left)
        List.Columns.Add("Название", 300, HorizontalAlignment.Left)
        List.Columns.Add("Нужно", 90, HorizontalAlignment.Right)
        List.Columns.Add("Осталось", 90, HorizontalAlignment.Right)
        If IncludeBuildColumns Then
            List.Columns.Add("Волна", 60, HorizontalAlignment.Right)
            List.Columns.Add("Этап", 110, HorizontalAlignment.Left)
            List.Columns.Add("Циклы", 80, HorizontalAlignment.Right)
            List.Columns.Add("Будет", 90, HorizontalAlignment.Right)
            List.Columns.Add("Остаток", 90, HorizontalAlignment.Right)
            List.Columns.Add("ME", 55, HorizontalAlignment.Right)
        Else
            List.Columns.Add("ME", 55, HorizontalAlignment.Right)
        End If
    End Sub

    Private Sub SetupStagesList()
        lstStages.Dock = DockStyle.Fill
        lstStages.View = View.Details
        lstStages.FullRowSelect = True
        lstStages.GridLines = True
        lstStages.HideSelection = False
        lstStages.Columns.Add("TypeID", 0, HorizontalAlignment.Left)
        lstStages.Columns.Add("Волна", 60, HorizontalAlignment.Right)
        lstStages.Columns.Add("Этап", 120, HorizontalAlignment.Left)
        lstStages.Columns.Add("Название", 270, HorizontalAlignment.Left)
        lstStages.Columns.Add("Нужно", 80, HorizontalAlignment.Right)
        lstStages.Columns.Add("Циклы", 70, HorizontalAlignment.Right)
        lstStages.Columns.Add("Будет", 80, HorizontalAlignment.Right)
        lstStages.Columns.Add("Остаток", 80, HorizontalAlignment.Right)
        lstStages.Columns.Add("Зависит от", 220, HorizontalAlignment.Left)
    End Sub

    Private Sub LoadProjectList()
        Projects = CurrentProjectStore.LoadProjects()
        lstProjects.Items.Clear()
        For Each Project In Projects
            lstProjects.Items.Add(Project.Name & "  (" & Project.Created.ToString("dd.MM.yyyy HH:mm") & ")")
        Next

        If lstProjects.Items.Count > 0 Then
            lstProjects.SelectedIndex = 0
        Else
            ClearLists()
        End If
    End Sub

    Private Sub lstProjects_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstProjects.SelectedIndexChanged
        If lstProjects.SelectedIndex < 0 OrElse lstProjects.SelectedIndex >= Projects.Count Then
            Return
        End If

        Dim Project As CurrentProject = Projects(lstProjects.SelectedIndex)
        LoadRows(lstItems, Project.Items, False)
        LoadStages(Project.BuildItems)
        LoadRows(lstBuild, Project.BuildItems, True)
        LoadRows(lstBuy, Project.BuyItems, False)
    End Sub

    Private Sub LoadRows(List As ListView, Rows As List(Of CurrentProjectRow), IncludeBuildColumns As Boolean)
        List.Items.Clear()
        For Each Row In Rows.OrderBy(Function(ProjectRow) If(ProjectRow.Wave <= 0, 1, ProjectRow.Wave)).
                            ThenBy(Function(ProjectRow) ProjectRow.StageName).
                            ThenBy(Function(ProjectRow) ProjectRow.Name)
            Dim Item As ListViewItem = List.Items.Add(CStr(Row.TypeID))
            Item.SubItems.Add(Row.Name)
            Item.SubItems.Add(FormatNumber(Row.Needed, 0))
            Item.SubItems.Add(FormatNumber(Row.Remaining, 0))
            If IncludeBuildColumns Then
                Item.SubItems.Add(CStr(If(Row.Wave <= 0, 1, Row.Wave)))
                Item.SubItems.Add(If(Trim(Row.StageName) = "", "Производство", Row.StageName))
                Item.SubItems.Add(FormatNumber(Row.Cycles, 0))
                Item.SubItems.Add(FormatNumber(Row.Produced, 0))
                Item.SubItems.Add(FormatNumber(Row.Excess, 0))
                Item.SubItems.Add(Row.MEValue)
            Else
                Item.SubItems.Add(Row.MEValue)
            End If
        Next
    End Sub

    Private Sub LoadStages(Rows As List(Of CurrentProjectRow))
        lstStages.Items.Clear()

        Dim SortedRows = Rows.OrderBy(Function(Row) If(Row.Wave <= 0, 1, Row.Wave)).
                              ThenBy(Function(Row) Row.StageName).
                              ThenBy(Function(Row) Row.Name)

        Dim LastWave As Integer = -1
        For Each Row In SortedRows
            Dim Wave As Integer = If(Row.Wave <= 0, 1, Row.Wave)
            If Wave <> LastWave Then
                Dim Header As ListViewItem = lstStages.Items.Add("0")
                Header.SubItems.Add(CStr(Wave))
                Header.SubItems.Add("Волна " & Wave)
                Header.SubItems.Add(If(Wave = 1, "Можно запускать сразу", "После завершения предыдущих волн"))
                Header.SubItems.Add("")
                Header.SubItems.Add("")
                Header.SubItems.Add("")
                Header.SubItems.Add("")
                Header.SubItems.Add("")
                Header.BackColor = Color.Gainsboro
                Header.Font = New Font(lstStages.Font, FontStyle.Bold)
                LastWave = Wave
            End If

            Dim Item As ListViewItem = lstStages.Items.Add(CStr(Row.TypeID))
            Item.SubItems.Add(CStr(Wave))
            Item.SubItems.Add(If(Trim(Row.StageName) = "", "Производство", Row.StageName))
            Item.SubItems.Add(Row.Name)
            Item.SubItems.Add(FormatNumber(Row.Needed, 0))
            Item.SubItems.Add(FormatNumber(Row.Cycles, 0))
            Item.SubItems.Add(FormatNumber(Row.Produced, 0))
            Item.SubItems.Add(FormatNumber(Row.Excess, 0))
            Item.SubItems.Add(If(Row.Notes, ""))
        Next
    End Sub

    Private Sub ClearLists()
        lstItems.Items.Clear()
        lstStages.Items.Clear()
        lstBuild.Items.Clear()
        lstBuy.Items.Clear()
    End Sub

    Private Sub btnDeleteProject_Click(sender As Object, e As EventArgs) Handles btnDeleteProject.Click
        If lstProjects.SelectedIndex < 0 Then
            Return
        End If

        If MsgBox("Удалить выбранный проект?", CType(vbYesNo + vbQuestion, MsgBoxStyle), Me.Text) = vbYes Then
            Projects.RemoveAt(lstProjects.SelectedIndex)
            CurrentProjectStore.SaveProjects(Projects)
            LoadProjectList()
        End If
    End Sub

    Private Sub btnDeleteRow_Click(sender As Object, e As EventArgs) Handles btnDeleteRow.Click
        If lstProjects.SelectedIndex < 0 Then
            Return
        End If

        Dim ActiveList As ListView = TryCast(tabs.SelectedTab.Controls(0), ListView)
        If ActiveList Is Nothing OrElse ActiveList.SelectedItems.Count = 0 Then
            Return
        End If

        Dim Project As CurrentProject = Projects(lstProjects.SelectedIndex)
        Dim Rows As List(Of CurrentProjectRow)
        If ActiveList Is lstStages Then
            Rows = Project.BuildItems
        ElseIf ActiveList Is lstItems Then
            Rows = Project.Items
        ElseIf ActiveList Is lstBuild Then
            Rows = Project.BuildItems
        Else
            Rows = Project.BuyItems
        End If

        For Each Selected As ListViewItem In ActiveList.SelectedItems
            Dim TypeID As Long = CLng(Selected.SubItems(0).Text)
            If TypeID = 0 Then
                Continue For
            End If

            Dim Name As String
            If ActiveList Is lstStages Then
                Name = Selected.SubItems(3).Text
            Else
                Name = Selected.SubItems(1).Text
            End If

            Rows.RemoveAll(Function(Row) Row.TypeID = TypeID AndAlso Row.Name = Name)
        Next

        CurrentProjectStore.SaveProjects(Projects)
        lstProjects_SelectedIndexChanged(Nothing, EventArgs.Empty)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
