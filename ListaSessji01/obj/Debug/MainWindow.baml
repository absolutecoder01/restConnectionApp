   M S B A M L   `   `   `  ����H  DListaSessji01, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ListaSessji01ListaSessji01  5   	       ListaSessji01.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ListaSessji01     5   	   -   5          � WindowStartupLocation$  CenterScreen=�5	   	    � Title$ Lista Sesji WP��5
   	   $	��550��6    $
��1044��6-    � 	Resources 5         EndAllSessionsButtonStyle �� 
TargetType �� Property �� Value  HideCurrentSessionRowStyle)  #System.Windows.Controls.DataGridRow �� Triggers �� Binding �� Value   BooleanToVisibilityConverter  EndSessionButtonStyle	 �� 
TargetType
 ��Name �� CornerRadius �� Triggers P� Property P� Value �� 
TargetName��%�  &	        &	 �     &	 [    &	 n    �� 5   
   " ��62   .�5      ��  � ��
 �  ��6+   �� 5       � ��
 �����6+   �� 5       � ��$ 10q�6(   �� 5       � ��$ 5q�6'   5      �� 5   
   "  63    5      �� 5       6   �� *IsCurrentSession+ True6C   .a�5      ��  � y�$ 	Collapsed=�63   5      5      �� 5   
   6'   �� 5   
   " ��6.   .�5      ��  � �� �	#FFDD4C4C6+   �� 5       � ��
 �����6+   �� 5       � ��$
 Bold"�6+   �� 5       � ��$
 10,5q�6(   �� 5        � ��$ 5q�6'   �� 5!       � �� �	#FFAA00006,   �� 5"       � ��$ 1q�60   �� 5#       �@�Cursor$
 Hand��6'   �� 5$       � �� 5%      �� 5&      "	 ��6&   .R�5'      �� $
 border��6!   #��z��5(      #��z��5)      #��z��5*      $ 5��5+      ._�5,      �� $��Center=�6/   $��Center=�5-   '   5.       5/      P� 50       �@J�IsMouseOver . . �51   "   �� $ border��6)    � �� �	#FFAA00006S   52      P� 53       � �� . . �54   "   �� $ border��6)    � �� �	#FF8800006S   55      57      59      5:      5;      .��5<      � .+�5=   
   �� -   5         ��Name$ SessionsTabControl��5=      $��1��61   5>      5?      