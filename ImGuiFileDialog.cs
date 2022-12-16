/*
	Copyright 2020 Limeoats
	Original project: https://github.com/Limeoats/L2DFileDialog
	Changes by Vladimir Sigalkin https://github.com/Iam1337/ImGui-FileDialog
    Ported to C# by W.M.R Jap-A-Joe https://github.com/japajoe
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace ImGuiNET
{
    public enum ImGuiFileDialogType
    {
        OpenFile,
        SaveFile,
        Count
    }

    public enum ImGuiFileDialogSortOrder
    {
        Up,
        Down,
        None
    }

    public class ImFileDialogInfo
    {
        public string title;
        public ImGuiFileDialogType type;

        public string fileName;
        public DirectoryInfo directoryPath;
        public string resultPath;

        public bool refreshInfo;
        public UInt64 currentIndex;
        public List<FileInfo> currentFiles;
        public List<DirectoryInfo> currentDirectories;

        public ImFileDialogInfo()
        {
            this.currentFiles = new List<FileInfo>();
            this.currentDirectories = new List<DirectoryInfo>();
        }
    }

    public class ImGuiFileDialog
    {
        private static void RefreshInfo(ImFileDialogInfo dialogInfo)
        {
            dialogInfo.refreshInfo = false;
            dialogInfo.currentIndex = 0;
            dialogInfo.currentFiles.Clear();
            dialogInfo.currentDirectories.Clear();

            var directory = new DirectoryInfo(dialogInfo.directoryPath.FullName);

            dialogInfo.currentDirectories = directory.GetDirectories().ToList();
            dialogInfo.currentFiles = directory.GetFiles().ToList();
        }

        private static float initialSpacingColumn0 = 230.0f;
        private static float initialSpacingColumn1 = 80.0f;
        private static float initialSpacingColumn2 = 90.0f;
        private static ImGuiFileDialogSortOrder fileNameSortOrder = ImGuiFileDialogSortOrder.None;
        private static ImGuiFileDialogSortOrder sizeSortOrder = ImGuiFileDialogSortOrder.None;
        private static ImGuiFileDialogSortOrder dateSortOrder = ImGuiFileDialogSortOrder.None;
        private static ImGuiFileDialogSortOrder typeSortOrder = ImGuiFileDialogSortOrder.None;

        public static bool FileDialog(ref bool open, ImFileDialogInfo dialogInfo)
        {
            if (!open)
                return false;

            if (dialogInfo == null)
                return false;

            bool complete = false;

            ImGui.PushID(dialogInfo.GetHashCode());
            ImGui.SetNextWindowSize(new Vector2(740.0f, 410.0f), ImGuiCond.FirstUseEver);

            if (ImGui.Begin(dialogInfo.title, ref open, ImGuiWindowFlags.NoDocking))
            {
                if (dialogInfo.currentFiles.Count == 0 && dialogInfo.currentDirectories.Count == 0 || dialogInfo.refreshInfo)
                    RefreshInfo(dialogInfo);

                // Draw path
                ImGui.Text("Path: " + dialogInfo.directoryPath);

                float contentRegionWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

                ImGui.BeginChild("##browser", new Vector2(contentRegionWidth, 300), true, ImGuiWindowFlags.HorizontalScrollbar);
                ImGui.Columns(4);

                // Columns size
                if (initialSpacingColumn0 > 0)
                {
                    ImGui.SetColumnWidth(0, initialSpacingColumn0);
                    initialSpacingColumn0 = 0.0f;
                }
                if (initialSpacingColumn1 > 0)
                {
                    ImGui.SetColumnWidth(1, initialSpacingColumn1);
                    initialSpacingColumn1 = 0.0f;
                }
                if (initialSpacingColumn2 > 0)
                {
                    ImGui.SetColumnWidth(2, initialSpacingColumn2);
                    initialSpacingColumn2 = 0.0f;
                }

                // File Columns
                if (ImGui.Selectable("Name"))
                {
                    sizeSortOrder = ImGuiFileDialogSortOrder.None;
                    dateSortOrder = ImGuiFileDialogSortOrder.None;
                    typeSortOrder = ImGuiFileDialogSortOrder.None;
                    fileNameSortOrder = fileNameSortOrder == ImGuiFileDialogSortOrder.Down ? ImGuiFileDialogSortOrder.Up : ImGuiFileDialogSortOrder.Down;
                }
                ImGui.NextColumn();
                if (ImGui.Selectable("Size"))
                {
                    fileNameSortOrder = ImGuiFileDialogSortOrder.None;
                    dateSortOrder = ImGuiFileDialogSortOrder.None;
                    typeSortOrder = ImGuiFileDialogSortOrder.None;
                    sizeSortOrder = sizeSortOrder == ImGuiFileDialogSortOrder.Down ? ImGuiFileDialogSortOrder.Up : ImGuiFileDialogSortOrder.Down;
                }
                ImGui.NextColumn();
                if (ImGui.Selectable("Type"))
                {
                    fileNameSortOrder = ImGuiFileDialogSortOrder.None;
                    dateSortOrder = ImGuiFileDialogSortOrder.None;
                    sizeSortOrder = ImGuiFileDialogSortOrder.None;
                    typeSortOrder = typeSortOrder == ImGuiFileDialogSortOrder.Down ? ImGuiFileDialogSortOrder.Up : ImGuiFileDialogSortOrder.Down;
                }
                ImGui.NextColumn();
                if (ImGui.Selectable("Date"))
                {
                    fileNameSortOrder = ImGuiFileDialogSortOrder.None;
                    sizeSortOrder = ImGuiFileDialogSortOrder.None;
                    typeSortOrder = ImGuiFileDialogSortOrder.None;
                    dateSortOrder = dateSortOrder == ImGuiFileDialogSortOrder.Down ? ImGuiFileDialogSortOrder.Up : ImGuiFileDialogSortOrder.Down;
                }
                ImGui.NextColumn();

                // File Separator
                ImGui.Separator();

                // Sort directories
                var directories = dialogInfo.currentDirectories;

                if (fileNameSortOrder != ImGuiFileDialogSortOrder.None || sizeSortOrder != ImGuiFileDialogSortOrder.None || typeSortOrder != ImGuiFileDialogSortOrder.None)
                {
                    if (dateSortOrder == ImGuiFileDialogSortOrder.Down)
                    {
                        directories.Sort((a, b) => a.Name.Length > b.Name.Length ? 1 : 0);
                    }
                    else
                    {
                        directories.Sort((a, b) => a.Name.Length < b.Name.Length ? 1 : 0);
                    }
                }
                else if (dateSortOrder != ImGuiFileDialogSortOrder.None)
                {
                    if (dateSortOrder == ImGuiFileDialogSortOrder.Down)
                    {
                        directories.Sort((a, b) => a.LastWriteTime > b.LastWriteTime ? 1 : 0);
                    }
                    else
                    {
                        directories.Sort((a, b) => a.LastWriteTime < b.LastWriteTime ? 1 : 0);
                    }
                }

                // Sort files
                var files = dialogInfo.currentFiles;

                if (fileNameSortOrder != ImGuiFileDialogSortOrder.None)
                {
                    if (dateSortOrder == ImGuiFileDialogSortOrder.Down)
                    {
                        files.Sort((a, b) => a.Name.Length > b.Name.Length ? 1 : 0);
                    }
                    else
                    {
                        files.Sort((a, b) => a.Name.Length < b.Name.Length ? 1 : 0);
                    }
                }
                else if (sizeSortOrder != ImGuiFileDialogSortOrder.None)
                {
                    if (dateSortOrder == ImGuiFileDialogSortOrder.Down)
                    {
                        files.Sort((a, b) => a.Length > b.Length ? 1 : 0);
                    }
                    else
                    {
                        files.Sort((a, b) => a.Length < b.Length ? 1 : 0);
                    }
                }
                else if (typeSortOrder != ImGuiFileDialogSortOrder.None)
                {
                    if (dateSortOrder == ImGuiFileDialogSortOrder.Down)
                    {
                        files.Sort((a, b) => a.Extension.Length > b.Extension.Length ? 1 : 0);
                    }
                    else
                    {
                        files.Sort((a, b) => a.Extension.Length < b.Extension.Length ? 1 : 0);
                    }
                }
                else if (dateSortOrder != ImGuiFileDialogSortOrder.None)
                {
                    if (dateSortOrder == ImGuiFileDialogSortOrder.Down)
                    {
                        files.Sort((a, b) => a.LastWriteTime > b.LastWriteTime ? 1 : 0);
                    }
                    else
                    {
                        files.Sort((a, b) => a.LastWriteTime < b.LastWriteTime ? 1 : 0);
                    }
                }

                UInt64 index = 0;

                // Draw parent
                if (dialogInfo.directoryPath.Parent != null)
                {
                    contentRegionWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

                    if (ImGui.Selectable("..", dialogInfo.currentIndex == index, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(contentRegionWidth, 0)))
                    {
                        dialogInfo.currentIndex = index;

                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            dialogInfo.directoryPath = dialogInfo.directoryPath.Parent;
                            dialogInfo.refreshInfo = true;
                        }
                    }
                    ImGui.NextColumn();
                    ImGui.TextUnformatted("-");
                    ImGui.NextColumn();
                    ImGui.TextUnformatted("<parent>");
                    ImGui.NextColumn();
                    ImGui.TextUnformatted("-");
                    ImGui.NextColumn();

                    index++;
                }

                // Draw directories
                for (int i = 0; i < directories.Count; ++i)
                {
                    var directoryEntry = dialogInfo.currentDirectories[i];
                    var directoryPath = directoryEntry;
                    var directoryName = directoryEntry.Name;

                    contentRegionWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

                    if (ImGui.Selectable(directoryName, dialogInfo.currentIndex == index, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(contentRegionWidth, 0)))
                    {
                        dialogInfo.currentIndex = index;

                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            dialogInfo.directoryPath = directoryPath;
                            dialogInfo.refreshInfo = true;
                        }
                    }

                    ImGui.NextColumn();
                    ImGui.TextUnformatted("-");
                    ImGui.NextColumn();
                    ImGui.TextUnformatted("<directory>");
                    ImGui.NextColumn();

                    var lastWriteTime = directoryEntry.LastWriteTime;
                    ImGui.TextUnformatted(lastWriteTime.ToString());
                    ImGui.NextColumn();

                    index++;
                }

                // Draw files
                for (int i = 0; i < files.Count; ++i)
                {
                    var fileEntry = dialogInfo.currentFiles[i];
                    var filePath = fileEntry.FullName;
                    var fileName = fileEntry.Name;

                    contentRegionWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

                    if (ImGui.Selectable(fileName, dialogInfo.currentIndex == index, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(contentRegionWidth, 0)))
                    {
                        dialogInfo.currentIndex = index;
                        dialogInfo.fileName = fileName;
                    }

                    ImGui.NextColumn();
                    ImGui.TextUnformatted(fileEntry.Length.ToString());
                    ImGui.NextColumn();
                    ImGui.TextUnformatted(Path.GetExtension(filePath));
                    ImGui.NextColumn();

                    var lastWriteTime = fileEntry.LastWriteTime;
                    ImGui.TextUnformatted(lastWriteTime.ToString());
                    ImGui.NextColumn();

                    index++;
                }
                ImGui.EndChild();

                // Draw filename
                int fileNameBufferSize = 200;
                string fileNameBuffer = string.Empty;

                string fileNameStr = dialogInfo.fileName;
                int fileNameSize = fileNameStr.Length;

                if (fileNameSize >= fileNameBufferSize)
                    fileNameSize = fileNameBufferSize - 1;

                fileNameBuffer = fileNameStr.Substring(0, fileNameSize);

                contentRegionWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

                ImGui.PushItemWidth(contentRegionWidth);
                if (ImGui.InputText("File Name", ref fileNameBuffer, (uint)fileNameBufferSize))
                {
                    dialogInfo.fileName = fileNameBuffer;
                    dialogInfo.currentIndex = 0;
                }

                if (ImGui.Button("Cancel"))
                {
                    fileNameSortOrder = ImGuiFileDialogSortOrder.None;
                    sizeSortOrder = ImGuiFileDialogSortOrder.None;
                    typeSortOrder = ImGuiFileDialogSortOrder.None;
                    dateSortOrder = ImGuiFileDialogSortOrder.None;

                    dialogInfo.refreshInfo = false;
                    dialogInfo.currentIndex = 0;
                    dialogInfo.currentFiles.Clear();
                    dialogInfo.currentDirectories.Clear();

                    open = false;
                }

                ImGui.SameLine();

                if (dialogInfo.type == ImGuiFileDialogType.OpenFile)
                {
                    if (ImGui.Button("Open"))
                    {
                        dialogInfo.resultPath = Path.Combine(dialogInfo.directoryPath.FullName, dialogInfo.fileName);

                        if (System.IO.File.Exists(dialogInfo.resultPath))
                        {
                            fileNameSortOrder = ImGuiFileDialogSortOrder.None;
                            sizeSortOrder = ImGuiFileDialogSortOrder.None;
                            typeSortOrder = ImGuiFileDialogSortOrder.None;
                            dateSortOrder = ImGuiFileDialogSortOrder.None;

                            dialogInfo.refreshInfo = false;
                            dialogInfo.currentIndex = 0;
                            dialogInfo.currentFiles.Clear();
                            dialogInfo.currentDirectories.Clear();

                            complete = true;
                            open = false;
                        }
                    }
                }
                else if (dialogInfo.type == ImGuiFileDialogType.SaveFile)
                {
                    if (ImGui.Button("Save"))
                    {
                        dialogInfo.resultPath = Path.Combine(dialogInfo.directoryPath.FullName, dialogInfo.fileName);

                        if (System.IO.File.Exists(dialogInfo.resultPath))
                        {
                            fileNameSortOrder = ImGuiFileDialogSortOrder.None;
                            sizeSortOrder = ImGuiFileDialogSortOrder.None;
                            typeSortOrder = ImGuiFileDialogSortOrder.None;
                            dateSortOrder = ImGuiFileDialogSortOrder.None;

                            dialogInfo.refreshInfo = false;
                            dialogInfo.currentIndex = 0;
                            dialogInfo.currentFiles.Clear();
                            dialogInfo.currentDirectories.Clear();

                            complete = true;
                            open = false;
                        }
                    }
                }
            }

            ImGui.End();
            ImGui.PopID();

            return complete;
        }
    }
}
