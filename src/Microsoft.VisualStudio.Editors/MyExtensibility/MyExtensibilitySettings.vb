' Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

Option Strict On
Option Explicit On
Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports System.Xml

Imports EnvDTE

Imports EnvDTE90

Imports Microsoft.VisualStudio.Editors.Common
Imports Microsoft.VisualStudio.Editors.MyExtensibility.MyExtensibilityUtil

Namespace Microsoft.VisualStudio.Editors.MyExtensibility

    ''' ;MyExtensibilitySettings
    ''' <summary>
    ''' Contain the dictionary mapping between triggering assemblies and extension templates
    ''' for the current user's Visual Studio.
    ''' </summary>
    Friend Class MyExtensibilitySettings

#Region "Public methods"

        ''' ;New
        ''' <summary>
        ''' Create an instance of MyExtensibilitySettings. Triggering assembly settings is loaded
        ''' from the settings file in the specified folderPath.
        ''' </summary>
        Public Sub New(folderPath As String)
            Try ' Attempt to construct the setting file path. Ignore ArgumentException.
                _settingsFilePath = Path.Combine(folderPath, ASM_SETTINGS_FILE_NAME)
            Catch ex As ArgumentException
            End Try

            LoadAssemblySettings()
        End Sub

        ''' ;GetExtensionTemplates
        ''' <summary>
        ''' Get the list of extension templates associated with the given assembly for the given project's type.
        ''' This list will contains only extensions with the latest version.
        ''' </summary>
        Public Function GetExtensionTemplates(
                projectTypeID As String, project As Project, assemblyFullName As String) _
                As List(Of MyExtensionTemplate)
            If project Is Nothing Then
                Return Nothing
            End If
            assemblyFullName = IIf(assemblyFullName Is Nothing, String.Empty, assemblyFullName.Trim())

            InitializeProjectKindSettings(projectTypeID, project)

            If _extensionInfos.ContainsKey(projectTypeID) Then
                Debug.Assert(_extensionInfos(projectTypeID) IsNot Nothing, "Corruped m_ExtensionInfos!")

                Dim extensionLists As List(Of MyExtensionTemplate) = _extensionInfos(projectTypeID).GetItems(assemblyFullName)
                If extensionLists Is Nothing OrElse extensionLists.Count <= 0 Then
                    Return extensionLists
                End If

                ' Only return the templates with the latest extension version.
                Dim templateDict As New Dictionary(Of String, MyExtensionTemplate)
                For Each template As MyExtensionTemplate In extensionLists
                    If templateDict.ContainsKey(template.ID) Then
                        If templateDict(template.ID).Version.CompareTo(template.Version) < 0 Then
                            templateDict(template.ID) = template
                        End If
                    Else
                        templateDict.Add(template.ID, template)
                    End If
                Next

                extensionLists.Clear()
                extensionLists.AddRange(templateDict.Values)

                Return extensionLists
            Else
                Return Nothing
            End If
        End Function

        ''' ;GetExtensionTemplates
        ''' <summary>
        ''' Get the list of extension templates for the given project's type.
        ''' </summary>
        Public Function GetExtensionTemplates(projectTypeID As String, project As Project) As List(Of MyExtensionTemplate)
            If project Is Nothing Then
                Return Nothing
            End If

            InitializeProjectKindSettings(projectTypeID, project)

            Dim result As New List(Of MyExtensionTemplate)

            If _extensionInfos.ContainsKey(projectTypeID) Then
                Dim asmDictionary As AssemblyDictionary(Of MyExtensionTemplate) = _extensionInfos(projectTypeID)
                If asmDictionary IsNot Nothing Then
                    Return asmDictionary.GetAllItems()
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End Function

        ''' ;GetExtensionTemplateNameAndDescription
        ''' <summary>
        ''' Given an ID and version, attempt to get the name and description 
        ''' of the extension template.
        ''' </summary>
        ''' <remarks>
        ''' CONSIDER: (HuyN) What to do with partial match (i.e id but not version).
        '''     Behavior now is to return nothing.
        ''' </remarks>
        Public Sub GetExtensionTemplateNameAndDescription(projectTypeID As String, project As Project,
                id As String, version As Version, assemblyName As String,
                ByRef name As String, ByRef description As String)
            name = Nothing
            description = Nothing

            If project Is Nothing Then
                Exit Sub
            End If
            If StringIsNullEmptyOrBlank(id) Then
                Exit Sub
            End If
            If version Is Nothing Then
                Exit Sub
            End If
            assemblyName = NormalizeAssemblyFullName(assemblyName)

            InitializeProjectKindSettings(projectTypeID, project)

            If Not _extensionInfos.ContainsKey(projectTypeID) Then
                Exit Sub
            End If
            Dim asmDictionary As AssemblyDictionary(Of MyExtensionTemplate) = _extensionInfos(projectTypeID)
            If asmDictionary IsNot Nothing Then

                Dim extensionTemplates As List(Of MyExtensionTemplate)

                If assemblyName IsNot Nothing Then
                    extensionTemplates = asmDictionary.GetItems(assemblyName)
                Else
                    extensionTemplates = asmDictionary.GetAllItems()
                End If
                If extensionTemplates IsNot Nothing Then
                    For Each extensionTemplate As MyExtensionTemplate In extensionTemplates
                        If StringEquals(extensionTemplate.ID, id) AndAlso version.Equals(extensionTemplate.Version) Then
                            name = extensionTemplate.DisplayName
                            description = extensionTemplate.Description
                            Exit Sub
                        End If
                    Next
                End If
            End If
        End Sub

        ''' ;GetAssemblyAutoAdd
        ''' <summary>
        ''' Return if the templates associated with the given assembly should be auto-added into the project
        ''' with the assembly.
        ''' </summary>
        Public Function GetAssemblyAutoAdd(assemblyFullName As String) As AssemblyOption
            Return GetAssemblyAutoOption(AddRemoveAction.Add, assemblyFullName)
        End Function

        ''' ;GetAssemblyAutoRemove
        ''' <summary>
        ''' Return if the templates associated with the given assembly should be auto-removed from the project
        ''' with the assembly.
        ''' </summary>
        Public Function GetAssemblyAutoRemove(assemblyFullName As String) As AssemblyOption
            Return GetAssemblyAutoOption(AddRemoveAction.Remove, assemblyFullName)
        End Function

        ''' ;SetAssemblyAutoAdd
        ''' <summary>
        ''' Save user's choice if the templates associated with the given assembly should be auto-added into the project
        ''' with the assembly.
        ''' </summary>
        Public Sub SetAssemblyAutoAdd(assemblyFullName As String, autoAdd As Boolean)
            SetAssemblyAutoOption(AddRemoveAction.Add, assemblyFullName, autoAdd)
        End Sub

        ''' ;SetAssemblyAutoRemove
        ''' <summary>
        ''' Save user's choice if the templates associated with the given assembly should be auto-removed from the project
        ''' with the assembly.
        ''' </summary>
        Public Sub SetAssemblyAutoRemove(assemblyFullName As String, autoRemove As Boolean)
            SetAssemblyAutoOption(AddRemoveAction.Remove, assemblyFullName, autoRemove)
        End Sub

#End Region

#Region "Private methods"

        ''' ;GetAssemblyAutoOption
        ''' <summary>
        ''' Get the given auto option for the given assembly.
        ''' </summary>
        Private Function GetAssemblyAutoOption(addOrRemove As AddRemoveAction, assemblyFullName As String) As AssemblyOption
            If StringIsNullEmptyOrBlank(assemblyFullName) Then
                Return AssemblyOption.Prompt
            End If
            assemblyFullName = NormalizeAssemblyFullName(assemblyFullName)
            If _autoOptions.ContainsKey(assemblyFullName) Then
                Dim asmAutoOption As AssemblyAutoOption = _autoOptions(assemblyFullName)
                Debug.Assert(asmAutoOption IsNot Nothing, "Corrupted m_AutoOptions!")
                Return IIf(addOrRemove = AddRemoveAction.Add, asmAutoOption.AutoAdd, asmAutoOption.AutoRemove)
            Else
                Return AssemblyOption.Prompt
            End If
        End Function

        ''' ;InitializeProjectKindSettings
        ''' <summary>
        ''' If this project kind has not been queried
        ''' - query VSCore for project item templates with custom data
        ''' - parse the custom data to add extension templates to mapping
        ''' </summary>
        Private Sub InitializeProjectKindSettings(projectTypeID As String, project As Project)
            If project Is Nothing Then
                Exit Sub
            End If

            If _extensionInfos.ContainsKey(projectTypeID) Then ' Settings for this project kind already initialized
                Exit Sub
            End If

            ' Get settings for this project kind.
            Dim solution3 As Solution3 = TryCast(project.DTE.Solution, Solution3)
            If solution3 Is Nothing Then
                Exit Sub
            End If

            Dim templatesWithCustomData As Templates = Nothing
            Try
                templatesWithCustomData = solution3.GetProjectItemTemplates(projectTypeID, CUSTOM_DATA_SIGNATURE)
            Catch ex As Exception When ReportWithoutCrash(ex, NameOf(InitializeProjectKindSettings), NameOf(MyExtensibilitySettings))
                ' Ignore exceptions.
            End Try
            If templatesWithCustomData Is Nothing OrElse templatesWithCustomData.Count = 0 Then
                Exit Sub
            End If

            Dim assemblyDictionary As New AssemblyDictionary(Of MyExtensionTemplate)()
            For Each template As Template In templatesWithCustomData
                Dim extensionTemplate As MyExtensionTemplate = MyExtensionTemplate.CreateInstance(template)
                If extensionTemplate IsNot Nothing Then
                    assemblyDictionary.AddItem(extensionTemplate.AssemblyFullName, extensionTemplate)
                End If
            Next
            If assemblyDictionary.GetAllItems() Is Nothing Then
                Exit Sub
            End If

            _extensionInfos.Add(projectTypeID, assemblyDictionary)
        End Sub

        ''' ;LoadAssemblySettings
        ''' <summary>
        ''' Load the settings for triggering assemblies if the setting file exists.
        ''' </summary>
        Private Sub LoadAssemblySettings()
            If Not File.Exists(_settingsFilePath) Then
                Exit Sub
            End If

            ' Read the settings file in one pass into a XmlDocument.
            Dim fileStream As FileStream = Nothing
            Dim xmlReader As XmlTextReader = Nothing
            Dim xmlDocument As New XmlDocument With {
                .XmlResolver = Nothing
            }
            Try
                fileStream = New FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                ' Required by Fxcop rule CA3054 - DoNotAllowDTDXmlTextReader
                xmlReader = New XmlTextReader(fileStream) With {
                    .DtdProcessing = DtdProcessing.Prohibit
                }
                While Not xmlReader.EOF
                    Dim xmlNode As XmlNode = xmlDocument.ReadNode(xmlReader)
                    xmlDocument.AppendChild(xmlNode)
                End While
            Catch ex As Exception
                ' Ignore exceptions.
            Finally
                If xmlReader IsNot Nothing Then
                    xmlReader.Close()
                End If
                If fileStream IsNot Nothing Then
                    fileStream.Close()
                End If
            End Try

            ' Process the XmlDocument to get triggering assemblies settings.
            Dim xmlElement As XmlElement = xmlDocument.DocumentElement
            If xmlElement Is Nothing Then
                Exit Sub
            End If
            For Each childNode As XmlNode In xmlElement.ChildNodes
                Dim assemblyElement As XmlElement = TryCast(childNode, XmlElement)
                If assemblyElement Is Nothing OrElse
                        Not StringEquals(assemblyElement.LocalName, ASSEMBLY_ELEMENT) Then
                    Continue For
                End If

                Dim assemblyFullName As String = NormalizeAssemblyFullName(
                    GetAttributeValue(assemblyElement, ASSEMBLY_FULLNAME_ATTRIBUTE))
                If StringIsNullEmptyOrBlank(assemblyFullName) OrElse _autoOptions.ContainsKey(assemblyFullName) Then
                    Continue For
                End If

                Dim autoAdd As AssemblyOption = ReadAssemblyOptionAttribute(assemblyElement, ASSEMBLY_AUTOADD_ATTRIBUTE)
                Dim autoRemove As AssemblyOption = ReadAssemblyOptionAttribute(assemblyElement, ASSEMBLY_AUTOREMOVE_ATTRIBUTE)

                _autoOptions.Add(assemblyFullName, New AssemblyAutoOption(autoAdd, autoRemove))
            Next
        End Sub

        ''' ;SaveAssemblySettings
        ''' <summary>
        ''' Save the settings for triggering assemblies.
        ''' </summary>
        Private Sub SaveAssemblySettings()
            Dim fileStream As FileStream = Nothing
            Dim xmlWriter As XmlTextWriter = Nothing
            Try
                fileStream = New FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None)
                xmlWriter = New XmlTextWriter(fileStream, Encoding.UTF8) With {
                    .Formatting = Formatting.Indented,
                    .Indentation = 2
                }

                xmlWriter.WriteStartDocument()
                xmlWriter.WriteStartElement(MY_EXTENSIONS_ELEMENT, MY_EXTENSIONS_ELEMENT_NAMESPACE)

                For Each entry As KeyValuePair(Of String, AssemblyAutoOption) In _autoOptions
                    xmlWriter.WriteStartElement(ASSEMBLY_ELEMENT)
                    xmlWriter.WriteAttributeString(ASSEMBLY_FULLNAME_ATTRIBUTE, entry.Key)
                    WriteAssemblyOptionAttribute(xmlWriter, ASSEMBLY_AUTOADD_ATTRIBUTE, entry.Value.AutoAdd)
                    WriteAssemblyOptionAttribute(xmlWriter, ASSEMBLY_AUTOREMOVE_ATTRIBUTE, entry.Value.AutoRemove)
                    xmlWriter.WriteEndElement()
                Next

                xmlWriter.WriteEndElement()
                xmlWriter.WriteEndDocument()
            Catch ex As Exception When ReportWithoutCrash(ex, NameOf(SaveAssemblySettings), NameOf(MyExtensibilitySettings))
                ' Ignore write exceptions.
            Finally
                If xmlWriter IsNot Nothing Then
                    xmlWriter.Close()
                ElseIf fileStream IsNot Nothing Then
                    fileStream.Close()
                End If
            End Try
        End Sub

        ''' ;SetAssemblyAutoOption
        ''' <summary>
        ''' Set the given auto option for the given assembly.
        ''' </summary>
        Private Sub SetAssemblyAutoOption(addOrRemove As AddRemoveAction, assemblyFullName As String, value As Boolean)
            If StringIsNullEmptyOrBlank(assemblyFullName) Then
                Exit Sub
            End If
            assemblyFullName = NormalizeAssemblyFullName(assemblyFullName)
            Dim inputValue As AssemblyOption = IIf(value, AssemblyOption.Yes, AssemblyOption.No)

            Dim asmAutoOption As AssemblyAutoOption = Nothing
            If _autoOptions.ContainsKey(assemblyFullName) Then
                asmAutoOption = _autoOptions(assemblyFullName)
                Debug.Assert(asmAutoOption IsNot Nothing, "Corrupted m_AutoOptions!")
                Dim existingValue As AssemblyOption = IIf(addOrRemove = AddRemoveAction.Add, asmAutoOption.AutoAdd, asmAutoOption.AutoRemove)
                If existingValue = inputValue Then
                    Exit Sub
                Else
                    If addOrRemove = AddRemoveAction.Add Then
                        asmAutoOption.AutoAdd = inputValue
                    Else
                        asmAutoOption.AutoRemove = inputValue
                    End If
                End If
            End If

            If asmAutoOption Is Nothing Then
                If addOrRemove = AddRemoveAction.Add Then
                    asmAutoOption = New AssemblyAutoOption(inputValue, AssemblyOption.Prompt)
                Else
                    asmAutoOption = New AssemblyAutoOption(AssemblyOption.Prompt, inputValue)
                End If
                _autoOptions.Add(assemblyFullName, asmAutoOption)
            End If

            SaveAssemblySettings()
        End Sub

#End Region

#Region "Private shared methods"

        ''' ;ReadAssemblyOptionAttribute
        ''' <summary>
        ''' Read an assembly option attribute from the given XML element.
        ''' Return AssemblyOption.Prompt if error occurred.
        ''' </summary>
        Private Shared Function ReadAssemblyOptionAttribute(xmlElement As XmlElement, attributeName As String) _
                As AssemblyOption
            Dim result As AssemblyOption = AssemblyOption.Prompt
            Dim attributeValue As String = GetAttributeValue(xmlElement, attributeName)
            If Not StringIsNullEmptyOrBlank(attributeValue) Then
                Try
                    result = DirectCast(AssemblyOptionConverter.ConvertFromInvariantString(attributeValue), AssemblyOption)
                Catch ex As Exception When ReportWithoutCrash(ex, NameOf(ReadAssemblyOptionAttribute), NameOf(MyExtensibilitySettings))
                End Try
            End If
            Return result
        End Function

        ''' ;WriteAssemblyOptionAttribute
        ''' <summary>
        ''' Write an AssemblyOption value into the specified attribute using the specified XML text writer.
        ''' Try to use TypeConverter.ConvertToInvariantString, if fail, use the numeric value.
        ''' </summary>
        Private Shared Sub WriteAssemblyOptionAttribute(writer As XmlTextWriter,
                attributeName As String, value As AssemblyOption)
            Debug.Assert(writer IsNot Nothing, "Null writer!")
            Debug.Assert(Not StringIsNullEmptyOrBlank(attributeName), "Null attribute name!")
            Debug.Assert(value = AssemblyOption.Yes OrElse value = AssemblyOption.No _
                OrElse value = AssemblyOption.Prompt, "Invalid value!")

            Dim text As String = Nothing
            Try
                text = AssemblyOptionConverter.ConvertToInvariantString(value)
            Catch ex As Exception When ReportWithoutCrash(ex, "Could not convert to invariant string", NameOf(MyExtensibilitySettings))
            End Try
            If text Is Nothing Then
                text = CInt(value).ToString()
            End If
            If text IsNot Nothing Then
                writer.WriteAttributeString(attributeName, text)
            End If
        End Sub

        ''' ;AssemblyOptionConverter
        ''' <summary>
        ''' Cached and lazy-init TypeConverter for AssemblyOption enumeration.
        ''' </summary>
        Private Shared ReadOnly Property AssemblyOptionConverter As TypeConverter
            Get
                If s_assemblyOptionConverter Is Nothing Then
                    s_assemblyOptionConverter = TypeDescriptor.GetConverter(GetType(AssemblyOption))
                End If
                Debug.Assert(s_assemblyOptionConverter IsNot Nothing, "Could not get converter for AssemblyOption!")
                Return s_assemblyOptionConverter
            End Get
        End Property

#End Region

        ' The extension dictionary: 
        ' - key is project GUID.
        ' - value is an AssemblyDictionary of object. AssemblyDictionary contains extension templates
        '   + key is an assembly full name.
        '   + enabling getting extension templates associated with an assembly with / without version.
        Private ReadOnly _extensionInfos As New Dictionary(Of String, AssemblyDictionary(Of MyExtensionTemplate))
        ' The auto options dictionary:
        ' - key is assembly full name without culture / public key - case insensitive.
        ' - value is AssemblyAutoOption.
        Private ReadOnly _autoOptions As New Dictionary(Of String, AssemblyAutoOption)(StringComparer.OrdinalIgnoreCase)

        ' Assembly settings file path
        Private ReadOnly _settingsFilePath As String
        ' Cached and lazy-init TypeConverter for AssemblyOption enumeration.
        Private Shared s_assemblyOptionConverter As TypeConverter

        ' Custom data signature in .vstemplate file
        Private Const CUSTOM_DATA_SIGNATURE As String = "Microsoft.VisualBasic.MyExtension"
        ' File name, element / attribute names for triggering assemblies settings.
        Private Const ASM_SETTINGS_FILE_NAME As String = "VBMyExtensionSettings.xml"
        Private Const MY_EXTENSIONS_ELEMENT As String = "VBMyExtensions"
        Private Const MY_EXTENSIONS_ELEMENT_NAMESPACE As String =
            "urn:schemas-microsoft-com:xml-msvbmyextensions"
        Private Const ASSEMBLY_ELEMENT As String = "Assembly"
        Private Const ASSEMBLY_FULLNAME_ATTRIBUTE As String = "FullName"
        Private Const ASSEMBLY_AUTOADD_ATTRIBUTE As String = "AutoAdd"
        Private Const ASSEMBLY_AUTOREMOVE_ATTRIBUTE As String = "AutoRemove"

    End Class
End Namespace
