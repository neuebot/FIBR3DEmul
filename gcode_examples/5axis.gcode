G54;
G92;

G0 X50 Y50 ; top left corner  
G1 F200 Y-50 A0	; up side
G1 F400 X-50 A0	; right side
G1 F600 Y50 A0	; bot side
G1 F800 X50 A0	; left side
G1 F1000 C135 A0	; first arc
G1 F1200 C270 A0	; second arc
G1 F1400 C0 A0	; third arc

G0 X0 Y0 Z100; 

G1 F200 X100 B30 ;
G1 F400 X90 B-30 ;
G1 F600 X0 C180 ;
G1 F800 B0 C0;	
G1 F200 X0 Y0 Z0;

