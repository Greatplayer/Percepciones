using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Office.Core;
using Percepciones.WPF.Entidades;

namespace Percepciones.WPF.Operacion
{
    public class CreateExcelDoc
    {

        private Microsoft.Office.Interop.Excel.Application app = null;
        private Microsoft.Office.Interop.Excel.Workbook workbook = null;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
        private Microsoft.Office.Interop.Excel.Range workSheet_range = null;

        public CreateExcelDoc()
        {
            createDoc();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            createDoc();
        }

        public void createDoc()
        {
            try
            {
                app = new Microsoft.Office.Interop.Excel.Application();
                app.Visible = true;
                workbook = app.Workbooks.Add(1);
                //worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
            }
            catch (Exception ex)
            {
                LogApp.LogError("Error en la inicializacion de archivo excel.", ex.InnerException.Message);
            }
        }

        public void CrearCabecerabloqueNuevo(int nroWorksheet, string[,] datos, int fila, int columna)
        {
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[nroWorksheet];
            Microsoft.Office.Interop.Excel.Range rangoEscribir = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[fila, columna];//fila, columna
            rangoEscribir = rangoEscribir.get_Resize(63, 7);
            rangoEscribir.RowHeight = 11.25;

            //rangoEscribir.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //rangoEscribir.Font.Bold = true;
            //rangoEscribir.ColumnWidth = 10;
            //rangoEscribir.set_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault, datos);

        }

        public void createHeaders(int row, int col, string htext, string cell1, string cell2, int mergeColumns, string b, bool font, int size, string fcolor)
        {
            worksheet.Cells[row, col] = htext;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge(mergeColumns);
            switch (b)
            {
                case "YELLOW":
                    workSheet_range.Interior.Color = System.Drawing.Color.Yellow.ToArgb();
                    break;
                case "GRAY":
                    workSheet_range.Interior.Color = System.Drawing.Color.Gray.ToArgb();
                    break;
                case "GAINSBORO":
                    workSheet_range.Interior.Color = System.Drawing.Color.Gainsboro.ToArgb();
                    break;
                case "Turquoise":
                    workSheet_range.Interior.Color = System.Drawing.Color.Turquoise.ToArgb();
                    break;
                case "PeachPuff":
                    workSheet_range.Interior.Color = System.Drawing.Color.PeachPuff.ToArgb();
                    break;
                default:
                    //  workSheet_range.Interior.Color = System.Drawing.Color..ToArgb();
                    break;
            }

            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Font.Bold = font;
            workSheet_range.ColumnWidth = size;
            if (fcolor.Equals(""))
            {
                workSheet_range.Font.Color = System.Drawing.Color.White.ToArgb();
            }
            else
            {
                workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();
            }
        }

        public void addData(int row, int col, string data, string cell1, string cell2, string format)
        {
            worksheet.Cells[row, col] = data;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.NumberFormat = format;
        }

        public void CrearCabecera(int row, int col, string htext, string cell1, string cell2,
                                  int mergeColumns, string b, bool font, int size, string fcolor, int nroWorksheet)
        {
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[nroWorksheet];
            worksheet.Cells[row, col] = htext;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge(mergeColumns);
            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Font.Bold = font;
            workSheet_range.ColumnWidth = size;

            if (fcolor.Equals(""))
            {
                workSheet_range.Font.Color = System.Drawing.Color.White.ToArgb();
            }
            else
            {
                workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();
            }
        }

        public void AgregarImagen()
        {

            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

            worksheet.Cells[1, 1] = "http://csharp.net-informations.com";
            worksheet.Cells[2, 1] = "Adding picture in Excel File";

            worksheet.Shapes.AddPicture(@"D:\image_1.JPG", MsoTriState.msoFalse, MsoTriState.msoCTrue, 50, 50, 300, 45);

            //worksheet.SaveAs("csharp.net-informations.xls", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal);
            workbook.Close(true);
            app.Quit();
        }

        public void CrearCabeceraBloque(int nroWorksheet, string[,] datos, int fila, int columna)
        {
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[nroWorksheet];
            Microsoft.Office.Interop.Excel.Range rangoEscribir = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[fila, columna];//fila, columna
            rangoEscribir = rangoEscribir.get_Resize(2, 7);
            rangoEscribir.Borders.Color = System.Drawing.Color.Black.ToArgb();
            rangoEscribir.Font.Bold = true;
            rangoEscribir.ColumnWidth = 10;
            rangoEscribir.set_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault, datos);

        }

        public void CrearCeldaNombreSaldo(int nroWorksheet, string[,] datos, int fila, int columna)
        {
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[nroWorksheet];
            Microsoft.Office.Interop.Excel.Range rangoEscribir = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[fila, columna];//fila, columna
            rangoEscribir = rangoEscribir.get_Resize(3, 7);
            rangoEscribir.Font.Bold = true;
            rangoEscribir.ColumnWidth = 10;
            rangoEscribir.set_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault, datos);

        }

        public void CrearCeldaEstadoCuenta(int nroWorksheet, string[,] datos, int fila, int columna, int tamFila, int tamColumna)
        {
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[nroWorksheet];
            Microsoft.Office.Interop.Excel.Range rangoEscribir = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[fila, columna];//fila, columna
            rangoEscribir = rangoEscribir.get_Resize(tamFila, tamColumna);
            rangoEscribir.ColumnWidth = 10;
            rangoEscribir.set_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault, datos);

        }

        public void CrearCelda(int row, int col, string htext, string cell1, string cell2,
                                  int mergeColumns, string b, bool font, int size, string fcolor, int nroWorksheet)
        {
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[nroWorksheet];
            worksheet.Cells[row, col] = htext;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge(mergeColumns);
            //workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Font.Bold = font;
            workSheet_range.ColumnWidth = size;

            if (fcolor.Equals(""))
            {
                workSheet_range.Font.Color = System.Drawing.Color.White.ToArgb();
            }
            else
            {
                workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();
            }
        }

        public void AgregarHoja()
        {
            app = new Microsoft.Office.Interop.Excel.Application();
            app.Visible = true;
            workbook = app.Workbooks.Add(1);
        }

    }
}
