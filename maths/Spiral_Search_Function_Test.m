clear
clc

global r;
r = 1; % Radius of sphere
r_i = 0.5; % Layer radius
global c;
c = pi*r/r_i; % Number of devisions

x = [];
y = [];
z = [];
s = [];

% s_max = r * sqrt( (c*c-1)*cos(2*pi) + c^2 +1 ) / (2^(3/2)) - r*c/2;

for a = 0:0.025:2*pi
    temp = (a-pi)/2;
    x = [x,r * cos(temp) * cos(c*a)];
    y = [y,r * cos(temp) * sin(c*a)];
    z = [z,r * sin(temp)];
    
    s = [s,t(a)];
end

scatter3(x,y,z);

function s = t(a)
    global c;
    global r;
    s = (r*sqrt(c^2*sin((a-pi)/2)^2+cos((a-pi)/2)^2))/2 - r*c/2;
end
