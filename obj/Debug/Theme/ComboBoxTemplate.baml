   M S B A M L   `   `   `  ����R  NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ComboBoxToggleButton  �� 
TargetType � ColumnDefinitions'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard �� TargetProperty �� 
TargetName8  2System.Windows.Media.Animation.EasingColorKeyFrame �� KeyTime �� Value	 ��Name
 �� CornerRadius �� EndPoint �� 
StartPoint  BorderLightColor � Color � Offset  BorderDarkColor  ControlLightColor  ControlMediumColor �� Color F�Name  
GlyphColor  ComboBoxTextBox �� 
TargetType �� Property �� Value R� KeyTime  System.Windows.Visibility   Visible R� Value   Hidden d�Name �� 	ClickMode d� 	IsChecked �� Mode �� RelativeSource ��Name J� IsHitTestVisible �� SelectionBoxItem  �� SelectionBoxItemTemplate! ��Name" �� 
IsReadOnly# �� 
IsReadOnly$ �Name% �� IsDropDownOpen& � AllowsTransparency' �Name( J� SnapsToDevicePixels) � ActualWidth* �� MaxDropDownHeight  BorderMediumColor+ K� IsItemsHost-  'System.Windows.Input.KeyboardNavigation,   DirectionalNavigation- �� Triggers. P� Property/ P� Value0 �� 
TargetName1 P� 
SourceName2 �� Padding�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml      5      %�  &	        &	 [    '�� �    7   7  '�� )    �� 5      "  d�5      .R�5   
   �  5      �� 5      �� 5      $��20 �6#    5
        5      CommonStates��. 5        
Normal��6"     5      	MouseOver��. 5      �� .��5      �� $N H(Panel.Background).(GradientBrush.GradientStops)[1].(GradientStop.Color)�6;   $ Border��5   +   .g�5   "     $ 0��66   $ 	#FF3843C4��5   $   5      5      5        5      Pressed��6"     5      Disabled��. 5      �� .��5      �� $N H(Panel.Background).(GradientBrush.GradientStops)[1].(GradientStop.Color)�6;   $ Border��5   +   .g�5   "     $ 0��66   $ 	#FFC5CBF9��5   $   5      �� 5      $* $(Shape.Fill).(SolidColorBrush.Color)�6;   $ Arrow��5   +   .g�5    "     $ 0��66   $ 	#FF888888��5!   $   5"      �� 5#      $P J(Border.BorderBrush).(GradientBrush.GradientStops)[1].(GradientStop.Color)�6;   $ Border��5$   +   .g�5%   "     $ 0��66   $ 	#FF888888��5&   $   5'      5(      5)      5*        5+      CheckStates��. 5,        Checked��. 5-      �� .��5.      �� $N H(Panel.Background).(GradientBrush.GradientStops)[1].(GradientStop.Color)�6;   $ Border��5/   +   .g�50   "     $ 0��66   $ 	#FF211AA9��51   $   52      53      54        55      	Unchecked��6"     56      Indeterminate��6"   57      .+�59      �� $	 Border��6   $��2��5:      $
 2��5;      $��1q�5<      ��5=      �� 5>      $	 0,1&�6*   $	 0,0&�5?      .��5@      � # �  6'   $ 0Q�5A      � 5B      # �  6'   $ 1Q�5C      5D      ��5F      �� 5H      $	 0,0&�6*   $	 0,1&�5I      ��5K      � � 5L   "   # �  6/   � 5M   "   # �  6/   $	 1.0Q�5N      5O      5Q      5T      �� 5U      $��0��6   $
 2,0,0,2��5V      $��1q�5W      ��5X      �� 5Y      # �  6&   5[      F� 5\      $ Arrow��6   $��1��5]      $��Center=�5^      $��Center=�5_      "����        � 	=  	= � z     	��5a      �� 5b      # �  6&   5d      5e      5f      �� 5h      "  ��5i      .R�5j   
   �� $	 PART_ContentHost��6   |�.  #��z��5l      5m      �� 5o      " ��5p      .�5q   
   ��  �@J�SnapsToDevicePixels . 5r      �� 5s   
    �@�OverridesDefaultStyle . 5t      �� 5u   
    � ��$
 Auto=�5v      �� 5w   
    � ��$
 Auto=�5x      �� 5y   
    � �� . 5z      �� 5{   
    � ��$	 120��5|      �� 5}   
    � ��$ 20��5~      �� 5   
    � �� 5�      �� 5�      "  ��6"   .R�5�      �  5�        5�      CommonStates��. 5�   "     
Normal��6.     5�   "   	MouseOver��6.     5�   "   Disabled��. 5�   &   �� .��5�   *   �� $ PART_EditableTextBox��6G   $6 0(TextElement.Foreground).(SolidColorBrush.Color)�5�   1   .g�5�   .     $ 0��6B   $ 	#FF888888��5�   *   5�   +   5�   '   5�   #   5�        5�      
EditStates��. 5�   "     Editable��. 5�   &   �� .��5�   *   T� $ (UIElement.Visibility)�6H   $ PART_EditableTextBox��5�   2   ..�5�   .   i� $ 0��6E   # Z 5�   -   5�   +   T� 5�   *   $ (UIElement.Visibility)�5�      $ ContentSite��5�   2   ..�5�   .   i� $ 0��6E   # Z 5�   -   5�   +   5�   '   5�   #     5�   "   
Uneditable��6.   5�      .+�5�      d� $ ToggleButton��6'   8��  $��2��5�      |�.  $ Press=�5�       5�      �� *IsDropDownOpen+$ TwoWay=� �� *TemplatedParent+�� 5�      $ ContentSite��6+    .  #��z 5�      #��z  5�      #��z��5�      $��3,3,23,3q�5�      $��Stretch=�5�      $
��Left=�5�      5�      �� 5�      $! PART_EditableTextBox��6"   8�� $
��Left=�5�      $��Bottom=�5�      $��3,3,23,3q�5�      |�. 
������ $y�Hidden=�5�      ## z" 5�      ��5�      Z� 5�      � 5�      $$ Popup��6    $��Bottom=�5�      #��z% 5�      & . |�.  $��Slide=�5�      .��5�      � $' DropDown��6#   ( . #��z) 5�      #��z* 5�      .+�5�   "   �� $	 DropDownBorder��6)   $��1q�5�      ��5�   &   �� 5�   *   # �  6:   ��5�   &   �� 5�   *   # �  6:   5�   #   �� 5�   "   $��4,6,4,6q�6/   ( . .��5�   &   �� + . $, 	Contained=�5�   1   5�   #   5�      5�      5�      - 5�      P� 5�      . �@��HasItems/ .  . �5�      �� $0 DropDownBorder��6%    � ��$ 95��5�   %   5�      P� 5�      . �@��
IsGrouping/ . . �5�      ��  � �� .  5�   %   5�      P� 5�      $1 Popup��6"   . �@�AllowsTransparency/ . . �5�      �� $0 DropDownBorder��6%    �@��CornerRadius$ 4��5�   %   �� 5�      $0 DropDownBorder��6%    � ��$ 0,2,0,0q�5�   %   5�      5�      5�      5�      �� 5�      " ��5�      .�5�   
   ��  �@J�SnapsToDevicePixels . 5�      �� 5�   
    �@�OverridesDefaultStyle . 5�      �� 5�   
    � �� 5�      �� 5�      "  ��6"   .R�5�      �� $	 Border��6   $2 2q�5�      ( . 
������  5       5     SelectionStates��. 5  "     
Unselected��6.     5  "   Selected��. 5  &   �� .��5  *   �� $ Border��6G   $0 *(Panel.Background).(SolidColorBrush.Color)�5  G   .g�5	  .     $ 0��6B   $ 	#FFC5CBF9��5
  B   5  +   5  '   5  #     5  "   SelectedUnfocused��. 5  &   �� .��5  *   �� $ Border��6G   $0 *(Panel.Background).(SolidColorBrush.Color)�5  G   .g�5  .     $ 0��6B   $ 	#FFDDDDDD��5  B   5  +   5  '   5  #   5     ._�5     �� 5     5     5     5     5     