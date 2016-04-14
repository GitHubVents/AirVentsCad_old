using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace VentsCadLibrary
{
    public partial class VentsCad
    {

        /// <summary>
        /// Папка с исходной моделью "Регулятора расхода воздуха". 
        /// </summary>
        private const string DamperFolder = @"\Библиотека проектирования\DriveWorks\11 - Damper";

        /// <summary>
        /// Папка для сохранения компонентов "Регулятора расхода воздуха". 
        /// </summary>
        private const string DamperDestinationFolder = @"\Проекты\Blauberg\11 - Регулятор расхода воздуха";


        #region Dumper

        /// <summary>
        /// Dumpers the s.
        /// </summary>
        /// <param name="typeOfFlange">The typeOfFlange.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="isOutDoor"></param>
        /// <param name="material"></param>
        /// <param name="newFile"></param>
        /// <param name="unloadXml"></param>
        /// <returns></returns>
        public void DumperS(string typeOfFlange, string width, string height, bool isOutDoor, string[] material, out string newFile, bool unloadXml)
        {
            newFile = null;
            if (!ConvertToInt(new[] { width, height })) return;

            var sourceFolder = LocalPath(VaultName);
            var destinationFolder = LocalPath(DestVaultName);

            string modelName;
            string modelDamperPath;
            string nameAsm;

            switch (typeOfFlange)
            {
                case "20":
                    modelName = "11-20";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11 - Damper";
                    break;
                case "30":
                    modelName = "11-30";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11-30";
                    break;
                default:
                    modelName = "11-20";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11 - Damper";
                    break;
            }

            var modelType = $"{(material[3] == "AZ" ? "" : "-" + material[3])}{(material[3] == "AZ" ? "" : material[1])}";

            var drawing = "11-20";
            if (modelName == "11-30")
            { drawing = modelName; }
            var newDamperName = modelName + "-" + width + "-" + height + modelType + (isOutDoor ? "-O" : "");
            var newDamperPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newDamperName}.SLDDRW";

            //if (OpenIfExist(newDamperPath)) return;

            // TODO Where to MOVE
            string path; int fileId; int projectId;
            if (GetExistingFile(Path.GetFileNameWithoutExtension(newDamperPath), out path, out fileId, out projectId))
            {
                if (MessageBox.Show("Вибровставка " + Path.GetFileNameWithoutExtension(newDamperPath) + " уже есть в базе. Открыть?",
                    "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("conisio://" + DestVaultName + "/open?projectid=" + projectId + "&documentid=" + fileId + "&objecttype=1");
                }
                return;
            }

            var modelDamperDrw = $@"{destinationFolder}{modelDamperPath}\{drawing}.SLDDRW";

            GetLastVersionAsmPdm(new FileInfo(modelDamperDrw).FullName, VaultName);

            if (!InitializeSw(true)) return;

            var swDocDrw = _swApp.OpenDoc6(@modelDamperDrw, (int)swDocumentTypes_e.swDocDRAWING,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);

            ModelDoc2 swDoc = _swApp.ActivateDoc2(nameAsm, false, 0);

            var swAsm = (AssemblyDoc)swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            _swApp.Visible = true;

            // Габариты
            var widthD = Convert.ToDouble(width);
            var heightD = Convert.ToDouble(height);
            // Количество лопастей
            var countL = (Math.Truncate(heightD / 100)) * 1000;

            // Шаг заклепок
            const double step = 140;
            var rivetW = (Math.Truncate(widthD / step) + 1) * 1000;
            var rivetH = (Math.Truncate(heightD / step) + 1) * 1000;

            // Высота уголков
            var hC = Math.Truncate(7 + 5.02 + (heightD - countL / 10 - 10.04) / 2);

            // Коэффициенты и радиусы гибов   
            var thiknessStr = material?[1].Replace(".", ",") ?? "0,8";
            //MessageBox.Show(thiknessStr);

            #region typeOfFlange = "20"

            if (typeOfFlange == "20")
            {
                if (Convert.ToInt32(countL / 1000) % 2 == 1) //нечетное
                {
                    swDoc.Extension.SelectByID2("Совпадение5918344", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918345", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение5918355", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918353", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditUnsuppress2();
                }

                string newName;
                string newPartPath;

                if (isOutDoor)
                {
                    swDoc.Extension.SelectByID2("Эскиз1", "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();

                    swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз34@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("ВНС-901.41.302-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-130@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-131@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-129@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-128@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-127@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-126@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    // 11-005 

                    newName = $"11-05-{height}-{modelType}";
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-005.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-005",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},

                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"D3@Эскиз37", (Convert.ToInt32(countL / 1000) % 2 == 1) ? "0" : "50"},

                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);

                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }

                    // 11-006 
                    newName = $"11-06-{height}-{modelType}";
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-006.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-006",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);

                        AddMaterial(material, newName);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }
                }
                else
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть12@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз35@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз36@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-003-6@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@11-003-6@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз34@11-003-6@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз35@11-003-6@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-187@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-188@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-189@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-190@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-191@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-192@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-193@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-194@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-195@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-196@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-197@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-198@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }


                // 11-001 
                newName = "11-01-" + height + modelType + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";

                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-001-7@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-001.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-001",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 8.04)},
                            {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз27", Convert.ToString(100*Math.Truncate(countL/2000))},
                            {"D1@Кривая1", Convert.ToString(countL)},

                            {"D1@Кривая2", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-002 
                newName = "11-03-" + width + modelType;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-002-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-002.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-002",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD - 3.96)},
                            {"D1@Кривая1", Convert.ToString(rivetW)},

                            {"D1@Кривая3", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл1", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-003 
                newName = "11-02-" + height + modelType + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-003-6@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-003.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-003",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 8.04)},
                            {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                            {"D1@Кривая1", Convert.ToString(countL)},

                            {"D1@Кривая2", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-004
                newName = "11-04-" + width + "-" + hC + modelType;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-004-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-004.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-004",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD - 24)},
                            {"D7@Ребро-кромка1", Convert.ToString(hC)},
                            {"D1@Кривая1", Convert.ToString(countL)},

                            {"D1@Кривая5", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл1", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                //11-100 Сборка лопасти
                var newNameAsm = "11-" + width;
                var newPartPathAsm =
                    $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                    _swApp.CloseDoc("11-100.SLDASM");
                }
                else
                {
                    #region  11-101  Профиль лопасти

                    newName = "11-" + (Math.Truncate(widthD - 23)) + "-01" + modelType;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        _swApp.IActivateDoc2("10-100", false, 0);
                        swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                        swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        _swApp.IActivateDoc2("10-100", false, 0);
                        swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0, null, 0);
                        var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                        myDimension.SystemValue = (widthD - 23) / 1000;
                        _swApp.ActivateDoc2("11-101", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(newName);

                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });

                        //NewComponents.Add(new VaultSystem.VentsCadFiles
                        //{
                        //    LocalPartFileInfo = new FileInfo(
                        //    $@"{destinationFolder}\{DamperDestinationFolder}\{newName}").FullName
                        //});
                    }

                    #endregion

                    _swApp.ActivateDoc2("11-100", false, 0);
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    swDoc.ForceRebuild3(false);
                    var docDrw100 = _swApp.OpenDoc6(
                        $@"{sourceFolder}{modelDamperPath}\{"11-100"}.SLDDRW", (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                    swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(newNameAsm);
                    docDrw100.ForceRebuild3(false);
                    docDrw100.SaveAs2(
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName) + " - DRW1");


                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    //NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo($@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName });
                }
            }

            #endregion

            #region typeOfFlange = "30"

            if (typeOfFlange == "30")
            {
                string newName;
                string newPartPath;

                if (isOutDoor)
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз27@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("ВНС-902.49.283-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-314@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-323@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-322@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-321@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-320@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-315@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-316@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-317@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-318@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-319@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    // 11-005 
                    newName = "11-05-" + height + modelType;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-005.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-005",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},

                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"D3@Эскиз37", (Convert.ToInt32(countL / 1000) % 2 == 1) ? "0" : "50"},

                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);
                        AddMaterial(material, newName);

                        //      NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });

                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }

                    // 11-006 
                    newName = "11-06-" + height + modelType;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-006.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-006",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                                {"D3@Эскиз1", Convert.ToString(heightD)},
                                {"D1@Кривая1", Convert.ToString(rivetH)},
                                {"Толщина@Листовой металл", thiknessStr}
                            },
                            false,
                            null);
                        AddMaterial(material, newName);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                    }
                }
                else
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть9@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз28@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз29@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-004-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@11-30-004-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз27@11-30-004-2@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз28@11-30-004-2@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Rivet Bralo-346@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-347@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-348@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-349@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-350@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-351@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-356@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-357@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-358@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-359@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-360@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-361@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }


                #region Кратность лопастей

                if (Convert.ToInt32(countL / 1000) % 2 == 0) //четное
                {
                    swDoc.Extension.SelectByID2("Совпадение5918344", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918345", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение5918355", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918353", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditUnsuppress2();
                }

                #endregion

                var lp = widthD - 50; // Размер профиля 640,5 при ширине двойного 1400 = 
                var lp2 = lp - 11.6; // Длина линии под заклепки профиля
                var lProfName = width; //
                var lProfNameLength = (widthD - 23) / 1000;

                #region IsDouble

                var isdouble = widthD > 1000;

                if (!isdouble)
                {
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-100-13@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-30-003-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-30-003-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ВНС-47.91.001-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент1", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("DerivedCrvPattern3", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-267@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-268@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-270@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-271@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-272@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-273@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-274@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-275@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-276@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-264@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-265@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-266@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-243@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-244@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-245@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-246@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-247@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-248@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-240@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-241@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-242@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-249@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-250@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-251@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                }

                if (isdouble)
                {
                    lp = widthD / 2 - 59.5;
                    lp2 = lp - 11.6;
                    lProfName = Convert.ToString(Math.Truncate(Convert.ToDouble(Convert.ToDouble(width)) / 2 - 9));
                    lProfNameLength = (widthD / 2 - 23) / 1000;
                }

                #endregion

                #region Детали

                // 11-30-001 
                newName = "11-30-03-" + width + modelType;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-001-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-30-001.SLDPRT");
                }
                else
                {
                    if (!isdouble)
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("11-30-001.SLDPRT", true, 0)));
                        swDoc.Extension.SelectByID2("Эскиз17", "SKETCH", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditSketch();
                        swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("D22@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D21@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D20@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D19@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D18@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D17@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D4@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D3@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D2@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D1@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditDelete();
                        swDoc.SketchManager.InsertSketch(true);
                    }

                    SwPartParamsChangeWithNewName("11-30-001",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD/2 - 0.8)},
                            {"D3@Эскиз18", Convert.ToString(lp2)},
                            {"D1@Кривая1", Convert.ToString((Math.Truncate(lp2/step) + 1)*1000)},
                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);
                    AddMaterial(material, newName);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-30-002 
                newName = "11-30-01-" + height + modelType + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-002-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-30-002.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-002",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                            {"D3@Эскиз23", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз23", Convert.ToString(100*Math.Truncate(countL/2000))},

                            {"D1@Кривая2", Convert.ToString(countL)},
                            {"D1@Кривая3", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                // 11-30-004 
                newName = "11-30-02-" + height + modelType + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-004-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-30-004.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-004",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                            {"D3@Эскиз23", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз23", Convert.ToString(100*Math.Truncate(countL/2000))},
                            {"D1@Кривая2", Convert.ToString(countL)},

                            {"D1@Кривая5", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }


                // 11-30-003 
                newName = "11-30-04-" + Math.Truncate(lp) + "-" + hC + modelType;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-003-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-30-003.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-003",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(lp)},
                            {"D7@Ребро-кромка1", Convert.ToString(hC)},
                            {"D1@Кривая1", Convert.ToString((Math.Truncate(lp2/step) + 1)*1000)},
                            {"Толщина@Листовой металл1", thiknessStr}
                        },
                        false,
                        null);

                    AddMaterial(material, newName);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                }

                #endregion

                #region Сборки

                #region 11-100 Сборка лопасти

                var newNameAsm = "11-2-" + lProfName;
                string newPartPathAsm =
                    $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";

                if (isdouble)
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        #region  11-101  Профиль лопасти

                        newName = "11-" + (Math.Truncate(lProfNameLength * 1000)) + "-01";
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-100.SLDASM");
                        }
                        else
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0,
                                null, 0);
                            var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = lProfNameLength;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                        }

                        #endregion
                    }
                }

                if (!isdouble)
                {
                    newNameAsm = "11-" + lProfName;
                    newPartPathAsm =
                        $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        #region  11-101  Профиль лопасти

                        newName = "11-" + (Math.Truncate(lProfNameLength * 1000)) + "-01";
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-100.SLDASM");
                        }
                        else
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0,
                                null, 0);
                            var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = lProfNameLength;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPath).FullName });
                        }

                        #endregion
                    }
                }

                _swApp.ActivateDoc2("11-100", false, 0);
                swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                if (isdouble)
                {
                    swDoc.Extension.SelectByID2("Совпадение76", "MATE", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение77", "MATE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ВНС-47.91.101-2@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }

                var docDrw100 = _swApp.OpenDoc6($@"{sourceFolder}{modelDamperPath}\{"11-100"}.SLDDRW", (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                swDoc.ForceRebuild3(false);
                swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newNameAsm);
                docDrw100.ForceRebuild3(false);
                docDrw100.SaveAs2(
                    $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(
                    $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName) + " - DRW1");

                NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(newPartPathAsm).FullName });
                NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo($@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName });

                #endregion

                #region 11-30-100 Сборка Перемычки

                newNameAsm = "11-30-100-" + height + modelType;
                newPartPathAsm = $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";

                if (isdouble)
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-30-100-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-30-100.SLDASM");
                    }
                    else
                    {
                        #region  11-30-101  Профиль перемычки

                        newName = "11-30-101-" + height + modelType;
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0, VaultName))
                        {
                            _swApp.IActivateDoc2("10-30-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-30-101-2@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-30-101",
                                $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                                    {"D3@Эскиз19", Convert.ToString(countL/10 - 100)},
                                    {"D1@Кривая1", Convert.ToString(countL)},
                                    {"D1@Кривая2", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr}
                                },
                                false,
                                null);
                            AddMaterial(material, newName);

                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo($@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName });
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-30-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(true);

                        #region  11-30-102  Профиль перемычки

                        newName = "11-30-102-" + height + modelType;
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0, VaultName))
                        {
                            _swApp.IActivateDoc2("10-30-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-30-102-2@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-30-102",
                                $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                                    {"D2@Эскиз19", Convert.ToString(countL/10 - 100)},
                                    {"D1@Кривая2", Convert.ToString(countL)},
                                    {"D1@Кривая1", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr}
                                },
                                false,
                                null);
                            try
                            {
                                VentsMatdll(material, null, newName);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }

                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo($@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName });
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-30-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(false); swDoc.ForceRebuild3(true);
                        swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(newNameAsm);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo($@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}").FullName });

                        #endregion

                    }
                }

                #endregion
            }

            #endregion

            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm, true, 0)));
            try
            {
                GabaritsForPaintingCamera(swDoc);
            }
            catch (Exception)
            {
                //
            }



            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            var name = $@"{destinationFolder}\{DamperDestinationFolder}\{newDamperName}";
            swDoc.SaveAs2(name + ".SLDASM", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(name + ".SLDASM").FullName));
            swDocDrw.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
            var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
            drw.ActivateSheet("DRW1");
            var m = 5;
            if (Convert.ToInt32(width) > 500 || Convert.ToInt32(height) > 500) { m = 10; }
            if (Convert.ToInt32(width) > 850 || Convert.ToInt32(height) > 850) { m = 15; }
            if (Convert.ToInt32(width) > 1250 || Convert.ToInt32(height) > 1250) { m = 20; }
            //drw.SetupSheet5("DRW1", 12, 12, 1, m, true, Settings.Default.DestinationFolder + @"\\srvkb\SolidWorks Admin\Templates\Основные надписи\A2-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);
            swDocDrw.SaveAs2(name + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(newDamperPath);
            _swApp.ExitApp();
            _swApp = null;


            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(name + ".SLDASM").FullName });
            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = new FileInfo(name + ".SLDDRW").FullName });

            List<VaultSystem.VentsCadFiles> outList;
            VaultSystem.CheckInOutPdmNew(NewComponents, true, DestVaultName, out outList);

            if (unloadXml)
            {
                foreach (var newComponent in NewComponents)
                {
                    PartInfoToXml(newComponent.LocalPartFileInfo);
                }
            }

            newFile = newDamperPath;
        }

        private void AddMaterial(string[] material, string newName)
        {
            try
            {
                //  VentsMatdll(material, null, newName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #endregion

        public void DumperS(string typeOfFlange, string width, string height, bool isOutDoor, out string newFile, bool unloadXml)
        {
            newFile = null;
            if (ConvertToInt(new[] { width, height }) == false) { return; }

            string modelName;
            string modelDamperPath;
            string nameAsm;

            switch (typeOfFlange)
            {
                case "20":
                    modelName = "11-20";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11 - Damper";
                    break;
                case "30":
                    modelName = "11-30";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11-30";
                    break;
                default:
                    modelName = "11-20";
                    modelDamperPath = DamperFolder;
                    nameAsm = "11 - Damper";
                    break;
            }

            var sourceFolder = LocalPath(VaultName);
            var destinationFolder = LocalPath(DestVaultName);


            var drawing = "11-20";
            if (modelName == "11-30")
            { drawing = modelName; }
            var newDamperName = modelName + "-" + width + "-" + height + (isOutDoor ? "-O" : "");
            var newDamperPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newDamperName}.SLDDRW";

            string path; int fileId; int projectId;
            if (GetExistingFile(newDamperName, out path, out fileId, out projectId))
            {
                if (MessageBox.Show("Вибровставка " + Path.GetFileNameWithoutExtension(newDamperPath) + " уже есть в базе. Открыть?",
                "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("conisio://" + DestVaultName + "/open?projectid=" + projectId + "&documentid=" + fileId + "&objecttype=1");
                }
                return;
            }

            var modelDamperDrw = $@"{sourceFolder}{modelDamperPath}\{drawing}.SLDDRW";

            GetLastVersionAsmPdm(modelDamperDrw, VaultName);

            if (!InitializeSw(true)) return;
            var swDocDrw = _swApp.OpenDoc6(@modelDamperDrw, (int)swDocumentTypes_e.swDocDRAWING,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);

            ModelDoc2 swDoc = _swApp.ActivateDoc2(nameAsm, false, 0);

            var swAsm = (AssemblyDoc)swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            _swApp.Visible = true;

            // Габариты
            var widthD = Convert.ToDouble(width);
            var heightD = Convert.ToDouble(height);
            // Количество лопастей
            var countL = (Math.Truncate(heightD / 100)) * 1000;

            // Шаг заклепок
            const double step = 140;
            var rivetW = (Math.Truncate(widthD / step) + 1) * 1000;
            var rivetH = (Math.Truncate(heightD / step) + 1) * 1000;

            // Высота уголков
            var hC = Math.Truncate(7 + 5.02 + (heightD - countL / 10 - 10.04) / 2);

            // Коэффициенты и радиусы гибов   
            const string thiknessStr = "1,0";
            // todo BendParams
            //var sbSqlBaseData = new SqlBaseData();
            //var bendParams = sbSqlBaseData.BendTable(thiknessStr);
            //var bendRadius = Convert.ToDouble(bendParams[0]);
            //var kFactor = Convert.ToDouble(bendParams[1]);


            #region typeOfFlange = "20"

            if (typeOfFlange == "20")
            {
                if (Convert.ToInt32(countL / 1000) % 2 == 1) //нечетное
                {
                    swDoc.Extension.SelectByID2("Совпадение5918344", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918345", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение5918355", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918353", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditUnsuppress2();
                }

                string newName;
                string newPartPath;

                if (isOutDoor)
                {
                    swDoc.Extension.SelectByID2("Эскиз1", "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();

                    swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз34@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("ВНС-901.41.302-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-130@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-131@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-129@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-128@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-127@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-126@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    // 11-005 
                    newName = "11-05-" + height;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-005.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-005",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                            {"D3@Эскиз1", Convert.ToString(heightD)},
                            {"D1@Кривая1", Convert.ToString(rivetH)},

                            {"D1@Кривая1", Convert.ToString(rivetH)},
                            {"D3@Эскиз37", (Convert.ToInt32(countL / 1000) % 2 == 1) ? "0" : "50"},

                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                            false,
                            null);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    }

                    // 11-006 
                    newName = "11-06-" + height;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-006.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-006",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                            {"D3@Эскиз1", Convert.ToString(heightD)},
                            {"D1@Кривая1", Convert.ToString(rivetH)},
                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                            false,
                            null);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    }
                }
                else
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть12@11-001-7@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз35@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз36@11-001-7@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть10@11-003-6@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть11@11-003-6@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз34@11-003-6@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз35@11-003-6@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-187@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-188@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-189@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-190@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-191@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-192@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-193@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-194@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-195@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-196@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-197@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-198@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }


                // 11-001 
                newName = "11-01-" + height + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";

                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-001-7@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-001.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-001",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 8.04)},
                            {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз27", Convert.ToString(100*Math.Truncate(countL/2000))},
                            {"D1@Кривая1", Convert.ToString(countL)},

                            {"D1@Кривая2", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    //newComponents.Add(new FileInfo(newPartPath));
                }

                // 11-002 
                newName = "11-03-" + width;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-002-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-002.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-002",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD - 3.96)},
                            {"D1@Кривая1", Convert.ToString(rivetW)},

                            {"D1@Кривая3", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл1", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл1", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл1", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }

                // 11-003 
                newName = "11-02-" + height + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-003-6@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-003.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-003",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 8.04)},
                            {"D1@Эскиз27", Convert.ToString(countL/10 - 100)},
                            {"D1@Кривая1", Convert.ToString(countL)},

                            {"D1@Кривая2", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }

                // 11-004 

                newName = "11-04-" + width + "-" + hC;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-004-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-004.SLDPRT");
                }
                else
                {
                    // todo BendParams
                    //bendParams = sbSqlBaseData.BendTable("0,8");
                    SwPartParamsChangeWithNewName("11-004",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD - 24)},
                            {"D7@Ребро-кромка1", Convert.ToString(hC)},
                            {"D1@Кривая1", Convert.ToString(countL)},

                            {"D1@Кривая5", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл1", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл1", Convert.ToString(Convert.ToDouble(bendParams[0]))},
                            //{"D2@Листовой металл1", Convert.ToString((Convert.ToDouble(bendParams[1]))*1000)}
                        },
                        false,
                        null);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }

                //11-100 Сборка лопасти

                var newNameAsm = "11-" + width;
                var newPartPathAsm =
                    $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                    _swApp.CloseDoc("11-100.SLDASM");
                }
                else
                {
                    #region  11-101  Профиль лопасти

                    newName = "11-" + (Math.Truncate(widthD - 23)) + "-01";
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        _swApp.IActivateDoc2("10-100", false, 0);
                        swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                        swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        _swApp.IActivateDoc2("10-100", false, 0);
                        swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                        swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0, null,
                            0);
                        var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                        myDimension.SystemValue = (widthD - 23) / 1000;
                        _swApp.ActivateDoc2("11-101", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(newName);

                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}" });
                    }

                    #endregion

                    _swApp.ActivateDoc2("11-100", false, 0);
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    swDoc.ForceRebuild3(false);
                    var docDrw100 = _swApp.OpenDoc6(
                        $@"{sourceFolder}{modelDamperPath}\{"11-100"}.SLDDRW", (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                    swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(newNameAsm);
                    docDrw100.ForceRebuild3(false);
                    docDrw100.SaveAs2(
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                    _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName) + " - DRW1");

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW" });
                }
            }

            #endregion

            #region typeOfFlange = "30"

            if (typeOfFlange == "30")
            {
                string newName;
                string newPartPath;

                if (isOutDoor)
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз27@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("ВНС-902.49.283-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-314@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-323@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-322@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-321@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-320@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Rivet Bralo-315@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-316@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-317@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-318@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-319@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    // 11-005 
                    newName = "11-05-" + height;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-005.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-005",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                            {"D3@Эскиз1", Convert.ToString(heightD)},
                            {"D1@Кривая1", Convert.ToString(rivetH)},

                            {"D1@Кривая1", Convert.ToString(rivetH)},
                            {"D3@Эскиз37", (Convert.ToInt32(countL / 1000) % 2 == 1) ? "0" : "50"},

                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                            false,
                            null);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    }

                    // 11-006 
                    newName = "11-06-" + height;
                    newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPath, "", false, true);
                        _swApp.CloseDoc("11-006.SLDPRT");
                    }
                    else
                    {
                        SwPartParamsChangeWithNewName("11-006",
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                            new[,]
                            {
                            {"D3@Эскиз1", Convert.ToString(heightD)},
                            {"D1@Кривая1", Convert.ToString(rivetH)},
                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                            },
                            false,
                            null);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                    }
                }
                else
                {
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть9@11-30-002-1@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз28@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз29@11-30-002-1@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Вырез-Вытянуть7@11-30-004-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вырез-Вытянуть8@11-30-004-2@" + nameAsm, "BODYFEATURE", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз27@11-30-004-2@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Эскиз28@11-30-004-2@" + nameAsm, "SKETCH", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("11-005-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-006-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Rivet Bralo-346@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-347@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-348@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-349@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-350@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-351@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-356@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-357@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-358@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-359@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-360@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-361@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }


                #region Кратность лопастей

                if (Convert.ToInt32(countL / 1000) % 2 == 0) //четное
                {
                    swDoc.Extension.SelectByID2("Совпадение5918344", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918345", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение5918355", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Совпадение5918353", "MATE", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditUnsuppress2();
                }

                #endregion

                var lp = widthD - 50; // Размер профиля 640,5 при ширине двойного 1400 = 
                var lp2 = lp - 11.6; // Длина линии под заклепки профиля
                var lProfName = width; //
                var lProfNameLength = (widthD - 23) / 1000;

                #region IsDouble

                var isdouble = widthD > 1000;

                if (!isdouble)
                {
                    swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                    swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.SelectByID2("11-30-100-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-100-13@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-30-003-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("11-30-003-4@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ВНС-47.91.001-1@11-30", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент1", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("DerivedCrvPattern3", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-267@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-268@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-270@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-271@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-272@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-273@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-274@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-275@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-276@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-264@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-265@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-266@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-243@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-244@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-245@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-246@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-247@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-248@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Rivet Bralo-240@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-241@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-242@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-249@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-250@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Rivet Bralo-251@11-30", "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                }

                if (isdouble)
                {
                    lp = widthD / 2 - 59.5;
                    lp2 = lp - 11.6;
                    lProfName = Convert.ToString(Math.Truncate(Convert.ToDouble(Convert.ToDouble(width)) / 2 - 9));
                    lProfNameLength = (widthD / 2 - 23) / 1000;
                }

                #endregion

                #region Детали

                // 11-30-001 
                newName = "11-30-03-" + width;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-001-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-30-001.SLDPRT");
                }
                else
                {
                    if (!isdouble)
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2("11-30-001.SLDPRT", true, 0)));
                        swDoc.Extension.SelectByID2("Эскиз17", "SKETCH", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditSketch();
                        swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("D22@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D21@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D20@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D19@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D18@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D17@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D4@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D3@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D2@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("D1@Эскиз17@11-30-001.SLDPRT", "DIMENSION", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditDelete();
                        swDoc.SketchManager.InsertSketch(true);
                    }

                    SwPartParamsChangeWithNewName("11-30-001",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(widthD/2 - 0.8)},
                            {"D3@Эскиз18", Convert.ToString(lp2)}, //
                            {"D1@Кривая1", Convert.ToString((Math.Truncate(lp2/step) + 1)*1000)},
                            {"Толщина@Листовой металл", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }

                // 11-30-002 
                newName = "11-30-01-" + height + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-002-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-30-002.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-002",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                            {"D3@Эскиз23", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз23", Convert.ToString(100*Math.Truncate(countL/2000))},

                            {"D1@Кривая2", Convert.ToString(countL)},
                            {"D1@Кривая3", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}

                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }

                // 11-30-004 
                newName = "11-30-02-" + height + (isOutDoor ? "-O" : "");
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-004-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", false, true);
                    _swApp.CloseDoc("11-30-004.SLDPRT");
                }
                else
                {
                    SwPartParamsChangeWithNewName("11-30-004",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                            {"D3@Эскиз23", Convert.ToString(countL/10 - 100)},
                            {"D2@Эскиз23", Convert.ToString(100*Math.Truncate(countL/2000))},
                            {"D1@Кривая2", Convert.ToString(countL)},

                            {"D1@Кривая5", Convert.ToString(rivetH)},

                            {"Толщина@Листовой металл", thiknessStr}

                            // todo BendParams
                            //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);
                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }


                // 11-30-003 
                newName = "11-30-04-" + Math.Truncate(lp) + "-" + hC;
                newPartPath = $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                {
                    swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                    swDoc.Extension.SelectByID2("11-30-003-2@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAsm.ReplaceComponents(newPartPath, "", true, true);
                    _swApp.CloseDoc("11-30-003.SLDPRT");
                }
                else
                {
                    //bendParams = sbSqlBaseData.BendTable("0,8");
                    //bendRadius = Convert.ToDouble(bendParams[0]);
                    //kFactor = Convert.ToDouble(bendParams[1]);
                    SwPartParamsChangeWithNewName("11-30-003",
                        $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                        new[,]
                        {
                            {"D2@Эскиз1", Convert.ToString(lp)},
                            {"D7@Ребро-кромка1", Convert.ToString(hC)},
                            {"D1@Кривая1", Convert.ToString((Math.Truncate(lp2/step) + 1)*1000)},
                            {"Толщина@Листовой металл1", thiknessStr},
                            // todo BendParams
                            //{"D1@Листовой металл1", Convert.ToString(bendRadius)},
                            //{"D2@Листовой металл1", Convert.ToString(kFactor*1000)}
                        },
                        false,
                        null);

                    NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                }

                #endregion

                #region Сборки

                #region 11-100 Сборка лопасти

                var newNameAsm = "11-2-" + lProfName;
                string newPartPathAsm =
                    $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";

                if (isdouble)
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        #region  11-101  Профиль лопасти

                        newName = "11-" + (Math.Truncate(lProfNameLength * 1000)) + "-01";
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-100.SLDASM");
                        }
                        else
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0,
                                null, 0);
                            var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = lProfNameLength;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        }

                        #endregion
                    }
                }

                if (!isdouble)
                {
                    newNameAsm = "11-" + lProfName;
                    newPartPathAsm =
                        $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-100-1@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-100.SLDASM");
                    }
                    else
                    {
                        #region  11-101  Профиль лопасти

                        newName = "11-" + (Math.Truncate(lProfNameLength * 1000)) + "-01";
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 1, VaultName))
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-101-1@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-100.SLDASM");
                        }
                        else
                        {
                            _swApp.IActivateDoc2("10-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("D1@Вытянуть1@11-101-1@11-100", "DIMENSION", 0, 0, 0, false, 0,
                                null, 0);
                            var myDimension = ((Dimension)(swDoc.Parameter("D1@Вытянуть1@11-101.Part")));
                            myDimension.SystemValue = lProfNameLength;
                            _swApp.ActivateDoc2("11-101", false, 0);
                            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                            swDoc.SaveAs2(newPartPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPath });
                        }

                        #endregion
                    }
                }

                _swApp.ActivateDoc2("11-100", false, 0);
                swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                if (isdouble)
                {
                    swDoc.Extension.SelectByID2("Совпадение76", "MATE", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("Совпадение77", "MATE", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ВНС-47.91.101-2@11-100", "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }

                var docDrw100 = _swApp.OpenDoc6($@"{sourceFolder}{modelDamperPath}\{"11-100"}.SLDDRW", (int)swDocumentTypes_e.swDocDRAWING, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
                swDoc.ForceRebuild3(false);
                swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(newNameAsm);
                docDrw100.ForceRebuild3(false);
                docDrw100.SaveAs2(
                    $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(
                    $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW").FullName) + " - DRW1");

                NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = newPartPathAsm });
                NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}.SLDDRW" });

                #endregion

                #region 11-30-100 Сборка Перемычки

                newNameAsm = "11-30-100-" + height;
                newPartPathAsm = $@"{destinationFolder}{DamperDestinationFolder}\{newNameAsm}.SLDASM";

                if (isdouble)
                {
                    if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPathAsm), 0, VaultName))
                    {
                        swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm + ".SLDASM", true, 0)));
                        swDoc.Extension.SelectByID2("11-30-100-4@" + nameAsm, "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swAsm.ReplaceComponents(newPartPathAsm, "", true, true);
                        _swApp.CloseDoc("11-30-100.SLDASM");
                    }
                    else
                    {
                        #region  11-30-101  Профиль перемычки

                        newName = "11-30-101-" + height;
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0, VaultName))
                        {
                            _swApp.IActivateDoc2("10-30-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-30-101-2@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-30-101",
                                $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                                    {"D3@Эскиз19", Convert.ToString(countL/10 - 100)},
                                    {"D1@Кривая1", Convert.ToString(countL)},
                                    {"D1@Кривая2", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr},
                                    // todo BendParams
                                    //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                                    //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                                },
                                false,
                                null);
                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}" });
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-30-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(true);

                        #region  11-30-102  Профиль перемычки

                        newName = "11-30-102-" + height;
                        newPartPath =
                            $@"{destinationFolder}\{DamperDestinationFolder}\{newName}.SLDPRT";
                        if (GetExistingFile(Path.GetFileNameWithoutExtension(newPartPath), 0, VaultName))
                        {
                            _swApp.IActivateDoc2("10-30-100", false, 0);
                            swDoc = (ModelDoc2)((IModelDoc2)(_swApp.ActiveDoc));
                            swDoc.Extension.SelectByID2("11-30-102-2@11-30-100", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                            swAsm.ReplaceComponents(newPartPath, "", true, true);
                            _swApp.CloseDoc("11-30-100.SLDASM");
                        }
                        else
                        {
                            SwPartParamsChangeWithNewName("11-30-102",
                                $@"{destinationFolder}\{DamperDestinationFolder}\{newName}",
                                new[,]
                                {
                                    {"D2@Эскиз1", Convert.ToString(heightD + 10)},
                                    {"D2@Эскиз19", Convert.ToString(countL/10 - 100)},
                                    {"D1@Кривая2", Convert.ToString(countL)},
                                    {"D1@Кривая1", Convert.ToString(rivetH)},
                                    {"Толщина@Листовой металл", thiknessStr},
                                    // todo BendParams
                                    //{"D1@Листовой металл", Convert.ToString(bendRadius)},
                                    //{"D2@Листовой металл", Convert.ToString(kFactor*1000)}
                                },
                                false,
                                null);
                            _swApp.CloseDoc(newName);
                            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}" });
                        }

                        #endregion

                        _swApp.ActivateDoc2("11-30-100", false, 0);
                        swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
                        swDoc.ForceRebuild3(false); swDoc.ForceRebuild3(true);
                        swDoc.SaveAs2(newPartPathAsm, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
                        _swApp.CloseDoc(newNameAsm);
                        NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = $@"{destinationFolder}\{DamperDestinationFolder}\{newNameAsm}" });

                        #endregion
                    }
                }

                #endregion
            }

            #endregion

            swDoc = ((ModelDoc2)(_swApp.ActivateDoc2(nameAsm, true, 0)));

            GabaritsForPaintingCamera(swDoc);
            swDoc.EditRebuild3();
            swDoc.ForceRebuild3(true);
            var name = $@"{destinationFolder}\{DamperDestinationFolder}\{newDamperName}";
            swDoc.SaveAs2(name + ".SLDASM", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(Path.GetFileNameWithoutExtension(new FileInfo(name + ".SLDASM").FullName));
            swDocDrw.Extension.SelectByID2("DRW1", "SHEET", 0, 0, 0, false, 0, null, 0);
            var drw = (DrawingDoc)(_swApp.IActivateDoc3(drawing + ".SLDDRW", true, 0));
            drw.ActivateSheet("DRW1");
            var m = 5;
            if (Convert.ToInt32(width) > 500 || Convert.ToInt32(height) > 500) { m = 10; }
            if (Convert.ToInt32(width) > 850 || Convert.ToInt32(height) > 850) { m = 15; }
            if (Convert.ToInt32(width) > 1250 || Convert.ToInt32(height) > 1250) { m = 20; }
            //drw.SetupSheet5("DRW1", 12, 12, 1, m, true, Settings.Default.DestinationFolder + @"\\srvkb\SolidWorks Admin\Templates\Основные надписи\A2-A-1.slddrt", 0.42, 0.297, "По умолчанию", false);
            swDocDrw.SaveAs2(name + ".SLDDRW", (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            _swApp.CloseDoc(newDamperPath);
            _swApp.ExitApp();

            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = name + ".SLDASM" });
            NewComponents.Add(new VaultSystem.VentsCadFiles { LocalPartFileInfo = name + ".SLDDRW" });

            List<VaultSystem.VentsCadFiles> outList;
            VaultSystem.CheckInOutPdmNew(NewComponents, true, DestVaultName, out outList);

            //CheckInOutPdmNew(NewComponents, true, Settings.Default.TestPdmBaseName, out outList);
            if (unloadXml)
            {
                foreach (var newComponent in NewComponents)
                {
                    PartInfoToXml(newComponent.LocalPartFileInfo);
                }
            }

            _swApp = null;
            newFile = newDamperPath;
        }

    }
}
