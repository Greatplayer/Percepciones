   M S B A M L   `   `   `  ����   Percepciones.WPF"    Percepciones.WPF.Interfaz_v1   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$=  7British American Tobacco Peru - Calculo de Percepciones�� � Icon$1 +/Percepciones.WPF;component/Images/bat3.jpg��$	��600��$	��768��.���  � RowDefinitions �� $��0.95* ��� $��0.05* �.+��� -   $��tabControl1��$��Stretch=�$��Stretch=�$��0,0,0,0q�.@��� -   $��tabItem1��$��Ingreso Archivo��$��13(�$	��150��.���  �� $��0.35* ��� $��0.65* �.+�� $��Stretch=�$��Stretch=�$��0�� � ColumnDefinitions �� $��0.75* ��� $��0.25* �.+�� $��0��.+��� -   $��	groupBox3��$��Carga Archivos��$��Stretch=�$��0,0,0,0q�$��Stretch=�.���  �� $��0.20* ��� $
��0.5* ��� $��0.30* � �� $��0.25* ��� $��0.25* ��� $
��0.5* �.+��� -   $��Label1��$��Documentos :��$
��Left=�$��13(�$	��Top=�$��6,3,0,0q��� -   $��txtDireccion��{�.  $��	15,5,15,5q�$��Stretch=�$��Stretch=�$��1���� -   $��btnCargarArchivo��$��Examinar��$��	15,5,15,5q�$��Stretch=�$��Stretch=�$��2���� -   $��label10��$��Nota Credito :��$��13(�$
��Left=�$	��103��$��30��$	��Top=�$��1��$��5,6,0,0q��� -   $��txtDireccionNC��$��Stretch=�{�.  $��	15,5,15,5q�$��Stretch=�$��1��$��1���� -	   $��btnCargarNC��$��Examinar��$��	15,5,15,5q�$��Stretch=�$��Stretch=�$��2��$��1���� -
   $��btnVerificarInfo��$��Verificar Información��$��27��$
��Left=�$��49,7,0,0q�$	��Top=�$	��203��{�.  $��2��$��2���� -   $��
btnGenerar��$��Generar TXT��$��27��$��Right=�{�.  $��0,7,49,0q�$	��Top=�$	��203��$��2��$��1��$��2��� -   $��pgBar��$��2��$��25��$��112,38,57,0q�$	��Top=�$��1��$y�Hidden=�� $��1��.+��� -   $��	groupBox4��$��Ingrese Calculado Odyssey��$��Stretch=�$��Stretch=�$��4��.���  �� $
��0.2* ��� $
��0.3* ��� $
��0.2* ��� $
��0.3* �.+��� -   $��label2��$��Monto Documentos :��$��28��$
��Left=�$��6,3,0,0q�$	��Top=�$��2���� -   $��label4��$��Percepcion :��$��2��$��28��$
��Left=�$	��Top=�$��6,2,0,0q��� -   $��txtOdysseyMonto��$��1��$��Stretch=�$��
10,5,10,12q�$��Stretch=� � ContextMenu Z� �� -   $��txtOdysseyPerce��$��3��$��Stretch=�$��
10,5,10,12q�$��Stretch=� Z� � $��1��$��Stretch=�$��Stretch=� �� $
��0.5* ��� $
��0.5* �.+��� -   $��	groupBox1��$��Verificación de Errores��$��Stretch=�$��0,0,0,0q�$��Stretch=�$��0��.��� .+��� -   $��txtVerificacionInfo��$��Stretch=�$��10,10,10,10q�$��Stretch=�{�. $��9(�$�� �� �� IsUndoEnabled .  �� -   $��	groupBox2��$*��$Resumen Previo Documentos Ingresados��$��Stretch=�$��0,0,0,0q�$��Stretch=�$��1��.��� .+�&   System.Windows.Controls.DataGrid  -   $��dgResumenPrevio��   AutoGenerateColumns .     CanUserAddRows .     CanUserDeleteRows .  	   CanUserReorderColumns	 .  
   CanUserResizeColumns
 .     CanUserResizeRows .     
IsReadOnly . $��Stretch=�$��10,10,10,10q�$��Stretch=�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Detalle��   Width5  /System.Windows.Controls.DataGridLengthConverter$
 0.4* 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *	Detalle+  $ Monto��$
 0.3*  �� *MontoDocumento+ �� StringFormat$ {0:C}��   ElementStyle ��  �� 
TargetType" ��.���  �� Property � �� �� Value$ Right=�   HeaderStyle �� =  7System.Windows.Controls.Primitives.DataGridColumnHeader"  .���  � ��$ Stretch=���  � ��$ Center=�  $ Percepción��$
 0.3*  �� *MontoPercepcion+$ {0:C}�� �� " ��.���  � ��$ Right=� �� "  .���  � ��$ Stretch=���  � ��$ Center=��� -   $��tabItem2��$��
Resultados��$	��100��$��13(�.���  �� $
��0.5* ��� $
��0.5* �.+��� -   $��label9��$��Resumen de Operación��$��39��$
��Left=�$��20,0,0,0q�$	��Top=�$	��262��$��15(�  -   $��dgCalculoFinal�� .   .   .  	 .  
 .   .   . $��
20,32,20,7q�   $ Detalle��$
 0.4*  �� *	Detalle+  $ Monto��$
 0.3*  �� *MontoDocumento+$ {0:C}�� �� " ��.���  � ��$ Right=� �� "  .���  � ��$ Stretch=���  � ��$ Center=�  $ Percepción��$
 0.3*  �� *MontoPercepcion+$ {0:C}�� �� " ��.���  � ��$ Right=� �� "  .���  � ��$ Stretch=���  � ��$ Center=�  -   $��dgNotaCredito�� .  $��
20,32,20,7q� .   .  	 .  
 .   .   . $��1��   $ Cod. Cliente��$ 0.15*  �� *CODIGO_CLIENTE+  $ Nota de Crédito��$ 0.15*  �� *	CODIGO_NC+  $ Fecha��$
 0.1*  �� *FECHA+ �� "  .���  � ��$ Stretch=���  � ��$ Center=� �� " ��.���  � ��$ Center=�  $ Monto��$ 0.18*  �� *MONTO+$ {0:C}�� �� "  .���  � ��$ Stretch=���  � ��$ Center=� �� " ��.���  � ��$ Right=�  $ Percepción��$ 0.12*  �� *
PERCEPCION+$ {0:C}�� �� "  .���  � ��$ Stretch=���  � ��$ Center=� �� " ��.���  � ��$ Right=�  $ Nombre Cliente��$ 0.35*  �� *NOMBRE_CLIENTE+�� -   $��label3��$#��Notas de Credito No Aplicadas��$��15(�$��39��$
��Left=�$��20,0,0,0q�$	��Top=�$	��262��$��1���� -   $��tabItem3��$��Libro��$	��100��$��13(�.��� .+��� -   $��btnProcesarArchivo��$��Procesar Archivo��$��23��$
��Left=�$��
481,48,0,0q�$	��Top=�$	��108���� -   $��cboEjercicio��$��23��$
��Left=�$��
101,46,0,0q�$	��Top=�$	��120���� -   $��
cboPeriodo��$��23��$��331,46,285,0q�$	��Top=��� -   $��label5��$��Ejercicio :��$��28��$
��Left=�$��	20,46,0,0q�$	��Top=��� -    $��label6��$��Periodo:��$��28��$
��Left=�$��
252,46,0,0q�$	��Top=��� -!   $��txtRutaInputLibro��$��23��$
��Left=�$��	202,6,0,0q�$	��Top=�$	��377���� -"   $��label7��$��Ruta de Archivos : ��$��28��$
��Left=�$��20,4,0,0q�$	��Top=��� -#   $��	btnBuscar��$��Buscar��$��23��$
��Left=�$��	585,6,0,0q�$	��Top=�$	��112��� -$   $��pgProcesoLibro��$��24��$
��Left=�$��	20,94,0,0q�$	��Top=�$	��363���� -%   $��btnGenerarLibro��$��Generar Libro��$
��Left=�$��24��$��
607,47,0,0q�$	��Top=�$	��108��� $��1�� �� $��0.60* ��� $��0.25* ��� $��0.075* ��� $��0.075* �.+��� -&   $��stsNotificacion��$��Stretch=�$��Stretch=�$��0,0,0,0q�$��4���� -'   $��
txbMensaje��$��Stretch=�$��5,5,0,0q�$��Iniciado��$	��Top=�$
��auto��$��0���� -(   $��
txbProceso��$��Stretch=�$��5,5,0,0q�$	��Top=�$
��auto��$��1���� -)   $��txbDuracion��$��Stretch=�$��5,5,0,0q�$��	Duracion:��$	��Top=�$
��auto��$��2���� -*   $��	txbConteo��$��Stretch=�$��5,5,0,0q�$	��Top=�$
��auto��$��3��