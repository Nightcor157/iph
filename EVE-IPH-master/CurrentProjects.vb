Imports System.IO
Imports System.Data.SQLite
Imports System.Xml.Linq

Public Class CurrentProject
    Public Property Id As String
    Public Property Name As String
    Public Property Created As DateTime
    Public Property Items As New List(Of CurrentProjectRow)
    Public Property BuildItems As New List(Of CurrentProjectRow)
    Public Property BuyItems As New List(Of CurrentProjectRow)
End Class

Public Class CurrentProjectRow
    Public Property TypeID As Long
    Public Property Name As String
    Public Property StageName As String
    Public Property Wave As Integer
    Public Property Needed As Long
    Public Property Cycles As Long
    Public Property Produced As Long
    Public Property Excess As Long
    Public Property Remaining As Long
    Public Property MEValue As String
    Public Property Notes As String
End Class

Public Module CurrentProjectStore
    Private Const ProjectFileName As String = "CurrentProjects.xml"

    Private Function ProjectFilePath() As String
        Dim BasePath As String = DynamicFilePath
        If Trim(BasePath) = "" Then
            BasePath = Path.GetDirectoryName(Application.ExecutablePath)
        End If

        Dim SettingsPath As String = Path.Combine(BasePath, SettingsFolder)
        If Not Directory.Exists(SettingsPath) Then
            Directory.CreateDirectory(SettingsPath)
        End If

        Return Path.Combine(SettingsPath, ProjectFileName)
    End Function

    Public Function LoadProjects() As List(Of CurrentProject)
        Dim Projects As New List(Of CurrentProject)
        Dim FilePath As String = ProjectFilePath()

        If Not File.Exists(FilePath) Then
            Return Projects
        End If

        Dim Doc As XDocument = XDocument.Load(FilePath)
        For Each ProjectNode In Doc.Root.Elements("Project")
            Dim Project As New CurrentProject With {
                .Id = CStr(ProjectNode.Attribute("Id")),
                .Name = CStr(ProjectNode.Attribute("Name")),
                .Created = DateTime.Parse(CStr(ProjectNode.Attribute("Created")))
            }

            Project.Items = LoadRows(ProjectNode.Element("Items"))
            Project.BuildItems = LoadRows(ProjectNode.Element("BuildItems"))
            Project.BuyItems = LoadRows(ProjectNode.Element("BuyItems"))
            Projects.Add(Project)
        Next

        Return Projects
    End Function

    Public Sub SaveProjects(Projects As List(Of CurrentProject))
        Dim Doc As New XDocument(New XElement("CurrentProjects",
            From Project In Projects
            Select New XElement("Project",
                New XAttribute("Id", Project.Id),
                New XAttribute("Name", Project.Name),
                New XAttribute("Created", Project.Created.ToString("o")),
                SaveRows("Items", Project.Items),
                SaveRows("BuildItems", Project.BuildItems),
                SaveRows("BuyItems", Project.BuyItems))))

        Doc.Save(ProjectFilePath())
    End Sub

    Public Sub AddProjectFromShoppingList(ProjectName As String, SourceList As ShoppingList)
        Dim Projects As List(Of CurrentProject) = LoadProjects()
        Dim Project As CurrentProject = CreateProjectFromShoppingList(ProjectName, SourceList)
        Projects.Add(Project)
        SaveProjects(Projects)
    End Sub

    Private Function CreateProjectFromShoppingList(ProjectName As String, SourceList As ShoppingList) As CurrentProject
        Dim Project As New CurrentProject With {
            .Id = Guid.NewGuid().ToString(),
            .Name = ProjectName,
            .Created = Now
        }

        Dim ItemList As Materials = SourceList.GetFullItemList()
        If ItemList IsNot Nothing AndAlso ItemList.GetMaterialList IsNot Nothing Then
            For Each Item In ItemList.GetMaterialList
                Project.Items.Add(New CurrentProjectRow With {
                    .TypeID = Item.GetMaterialTypeID,
                    .Name = Item.GetMaterialName,
                    .Needed = Item.GetQuantity,
                    .Remaining = Item.GetQuantity,
                    .MEValue = Item.GetItemME
                })
            Next
        End If

        Dim BuildList As BuiltItemList = SourceList.GetFullBuildList()
        Dim BuildDependencies As New Dictionary(Of Long, List(Of Long))
        Dim BuildWaves As New Dictionary(Of Long, Integer)

        If BuildList IsNot Nothing AndAlso BuildList.GetBuiltItemList IsNot Nothing Then
            For Each BuildItem In BuildList.GetBuiltItemList
                BuildDependencies(BuildItem.ItemTypeID) = GetBuildDependencies(BuildItem, BuildList)
            Next

            For Each BuildItem In BuildList.GetBuiltItemList
                CalculateBuildWave(BuildItem.ItemTypeID, BuildDependencies, BuildWaves, New List(Of Long))
            Next

            For Each BuildItem In BuildList.GetBuiltItemList
                Dim Cycles As Long = 0
                Dim Produced As Long = 0
                Dim Excess As Long = 0

                If BuildItem.PortionSize > 0 Then
                    Cycles = CLng(Math.Ceiling(BuildItem.ItemQuantity / BuildItem.PortionSize))
                    Produced = Cycles * BuildItem.PortionSize
                    Excess = Produced - BuildItem.ItemQuantity
                End If

                Project.BuildItems.Add(New CurrentProjectRow With {
                    .TypeID = BuildItem.ItemTypeID,
                    .Name = BuildItem.ItemName,
                    .StageName = GetProductionStageName(BuildItem.ItemTypeID),
                    .Wave = If(BuildWaves.ContainsKey(BuildItem.ItemTypeID), BuildWaves(BuildItem.ItemTypeID), 1),
                    .Needed = BuildItem.ItemQuantity,
                    .Cycles = Cycles,
                    .Produced = Produced,
                    .Excess = Excess,
                    .Remaining = BuildItem.ItemQuantity,
                    .MEValue = CStr(BuildItem.BuildME),
                    .Notes = GetDependencyNames(BuildDependencies(BuildItem.ItemTypeID), BuildList)
                })
            Next
        End If

        Dim BuyList As Materials = SourceList.GetFullBuyList()
        If BuyList IsNot Nothing AndAlso BuyList.GetMaterialList IsNot Nothing Then
            For Each Material In BuyList.GetMaterialList
                Project.BuyItems.Add(New CurrentProjectRow With {
                    .TypeID = Material.GetMaterialTypeID,
                    .Name = Material.GetMaterialName,
                    .Needed = Material.GetQuantity,
                    .Remaining = Material.GetQuantity
                })
            Next
        End If

        Return Project
    End Function

    Private Function LoadRows(Parent As XElement) As List(Of CurrentProjectRow)
        Dim Rows As New List(Of CurrentProjectRow)
        If Parent Is Nothing Then
            Return Rows
        End If

        For Each RowNode In Parent.Elements("Row")
            Rows.Add(New CurrentProjectRow With {
                .TypeID = CLng(RowNode.Attribute("TypeID")),
                .Name = CStr(RowNode.Attribute("Name")),
                .StageName = GetOptionalString(RowNode, "Stage"),
                .Wave = GetOptionalInteger(RowNode, "Wave", 0),
                .Needed = CLng(RowNode.Attribute("Needed")),
                .Cycles = CLng(RowNode.Attribute("Cycles")),
                .Produced = CLng(RowNode.Attribute("Produced")),
                .Excess = CLng(RowNode.Attribute("Excess")),
                .Remaining = CLng(RowNode.Attribute("Remaining")),
                .MEValue = CStr(RowNode.Attribute("ME")),
                .Notes = CStr(RowNode.Attribute("Notes"))
            })
        Next

        Return Rows
    End Function

    Private Function SaveRows(NodeName As String, Rows As List(Of CurrentProjectRow)) As XElement
        Return New XElement(NodeName,
            From Row In Rows
            Select New XElement("Row",
                New XAttribute("TypeID", Row.TypeID),
                New XAttribute("Name", Row.Name),
                New XAttribute("Stage", If(Row.StageName, "")),
                New XAttribute("Wave", Row.Wave),
                New XAttribute("Needed", Row.Needed),
                New XAttribute("Cycles", Row.Cycles),
                New XAttribute("Produced", Row.Produced),
                New XAttribute("Excess", Row.Excess),
                New XAttribute("Remaining", Row.Remaining),
                New XAttribute("ME", If(Row.MEValue, "")),
                New XAttribute("Notes", If(Row.Notes, ""))))
    End Function

    Private Function GetBuildDependencies(BuildItem As BuiltItem, BuildList As BuiltItemList) As List(Of Long)
        Dim Dependencies As New List(Of Long)
        If BuildItem Is Nothing OrElse BuildList Is Nothing Then
            Return Dependencies
        End If

        If BuildItem.BuildMaterials IsNot Nothing AndAlso BuildItem.BuildMaterials.GetMaterialList IsNot Nothing Then
            For Each Material In BuildItem.BuildMaterials.GetMaterialList
                If Material.GetBuildItem AndAlso IsBuiltInProject(Material.GetMaterialTypeID, BuildList) Then
                    AddDependency(Dependencies, Material.GetMaterialTypeID)
                End If
            Next
        End If

        If BuildItem.ComponentBuildList IsNot Nothing Then
            For Each Component In BuildItem.ComponentBuildList
                If IsBuiltInProject(Component.ItemTypeID, BuildList) Then
                    AddDependency(Dependencies, Component.ItemTypeID)
                End If
            Next
        End If

        Dependencies.RemoveAll(Function(TypeID) TypeID = BuildItem.ItemTypeID)
        Return Dependencies
    End Function

    Private Function CalculateBuildWave(TypeID As Long, Dependencies As Dictionary(Of Long, List(Of Long)), Waves As Dictionary(Of Long, Integer), Stack As List(Of Long)) As Integer
        If Waves.ContainsKey(TypeID) Then
            Return Waves(TypeID)
        End If

        If Stack.Contains(TypeID) Then
            Return 1
        End If

        Stack.Add(TypeID)
        Dim Wave As Integer = 1
        If Dependencies.ContainsKey(TypeID) Then
            For Each DependencyID In Dependencies(TypeID)
                Wave = Math.Max(Wave, CalculateBuildWave(DependencyID, Dependencies, Waves, Stack) + 1)
            Next
        End If

        Stack.Remove(TypeID)
        Waves(TypeID) = Wave
        Return Wave
    End Function

    Private Function IsBuiltInProject(TypeID As Long, BuildList As BuiltItemList) As Boolean
        For Each ProjectBuildItem In BuildList.GetBuiltItemList
            If ProjectBuildItem.ItemTypeID = TypeID Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub AddDependency(Dependencies As List(Of Long), TypeID As Long)
        If Not Dependencies.Contains(TypeID) Then
            Dependencies.Add(TypeID)
        End If
    End Sub

    Private Function GetDependencyNames(DependencyIDs As List(Of Long), BuildList As BuiltItemList) As String
        Dim Names As New List(Of String)
        For Each DependencyID In DependencyIDs
            For Each ProjectBuildItem In BuildList.GetBuiltItemList
                If ProjectBuildItem.ItemTypeID = DependencyID Then
                    If Not Names.Contains(ProjectBuildItem.ItemName) Then
                        Names.Add(ProjectBuildItem.ItemName)
                    End If
                    Exit For
                End If
            Next
        Next

        Return String.Join(", ", Names.ToArray())
    End Function

    Private Function GetProductionStageName(TypeID As Long) As String
        Dim ItemGroup As String = ""
        Dim ItemCategory As String = ""

        Try
            DBCommand = New SQLiteCommand("SELECT ITEM_GROUP, ITEM_CATEGORY FROM INVENTORY_TYPES WHERE typeID = " & TypeID, EVEDB.DBREf)
            Dim Reader As SQLiteDataReader = DBCommand.ExecuteReader
            If Reader.Read Then
                ItemGroup = Reader.GetString(0)
                ItemCategory = Reader.GetString(1)
            End If
            Reader.Close()
        Catch
            Return "Производство"
        End Try

        If ItemGroup.Contains("Composite") OrElse ItemGroup.Contains("Hybrid Polymer") OrElse ItemGroup.Contains("Biochemical") _
            OrElse ItemGroup.Contains("Molecular-Forged") OrElse ItemGroup.Contains("Moon Materials") Then
            Return "Реакции"
        End If

        If ItemGroup.Contains("Component") OrElse ItemCategory.Contains("Component") Then
            Return "Компоненты"
        End If

        If ItemCategory = "Ship" Then
            Return "Корабли"
        End If

        If ItemCategory = "Module" OrElse ItemCategory = "Structure Module" Then
            Return "Модули"
        End If

        If ItemCategory.Contains("Structure") OrElse ItemGroup.Contains("Structure") Then
            Return "Структуры"
        End If

        If ItemCategory = "Charge" Then
            Return "Заряды"
        End If

        Return "Производство"
    End Function

    Private Function GetOptionalString(RowNode As XElement, AttributeName As String) As String
        If RowNode.Attribute(AttributeName) Is Nothing Then
            Return ""
        End If
        Return CStr(RowNode.Attribute(AttributeName))
    End Function

    Private Function GetOptionalInteger(RowNode As XElement, AttributeName As String, DefaultValue As Integer) As Integer
        If RowNode.Attribute(AttributeName) Is Nothing Then
            Return DefaultValue
        End If
        Return CInt(RowNode.Attribute(AttributeName))
    End Function
End Module
