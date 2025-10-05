# ImplicitPlotter

ImplicitPlotter is a command-line tool that can plot graph of any binary implicit function equation or inequality, supporting both Cartesian and polar coordinates.

The plotting algorithm is based on [interval arithmetic](https://www.dgp.toronto.edu/public_user/mooncake/papers/SIGGRAPH2001_Tupper.pdf).


## Usage

```
Usage:
  ImplicitPlotter [options]

Options:
  --relation <relation> (REQUIRED)     Implicit function expression to be plotted.
  --output <output> (REQUIRED)         Save path of the output image.
  --width <width>                      The width of the output image. [default: 500]
  --height <height>                    The height of the output image. [default: 500]
  --xmin <xmin>                        The minimum value of x. [default: -10]
  --xmax <xmax>                        The maximum value of x. [default: 10]
  --ymin <ymin>                        The minimum value of y. [default: -10]
  --ymax <ymax>                        The maximum value of y. [default: 10]
  --drawColor <drawColor>              Plotting color. [default: #C80078D7]
  --backgroundColor <backgroundColor>  Image background color. [default: #FFFFFFFF]
  --timeout <timeout>                  Draw timeout (seconds). [default: 10000]
```

## Example

### Curve

```shell
ImplicitPlotter --relation "sin(x^2-y^2)=sin(x+y)+cos(x*y)" --output "./output.png" --width 1000 --height 1000 --xmin -10 --xmax 10 --ymin -10 --ymax 10 --drawColor "#0078D7"
```

![Heart](./img/Curve.png)

### Heart

```shell
ImplicitPlotter --relation "17*x^2-16*abs(x)*y+17*y^2+150/abs(5*x+sin(5*y))<225" --output "./output.png" --width 1000 --height 1000 --xmin -6 --xmax 6 --ymin -6 --ymax 6 --drawColor "#ED4336"
```

![Heart](./img/Heart.png)

### Face

```shell
ImplicitPlotter --relation "1/15*(6-y)+1/6400000*(8*x^2+4*(y-3)^2)^3+cos(max((x+y)*cos(y-x),(y-x)*cos(x+y)))<sin(min((x+y)*sin(y-x),(y-x)*sin(x+y)))" --output "./output.png" --width 1000 --height 1000 --xmin -8 --xmax 8 --ymin -5 --ymax 11 --drawColor "#366AAD"
```

![Face](./img/Face.png)

### Tai Chi

```shell
ImplicitPlotter --relation "(cos(a-r)-sin(a))*(r^4-2*r^2*cos(2*a+2.4)+0.9)+(0.62*r)^1000<0" --output "./output.png" --width 1000 --height 1000 --xmin -3 --xmax 3 --ymin -3 --ymax 3 --drawColor "#000000"
```

![Tai Chi](./img/TaiChi.png)

### Pattern

```shell
ImplicitPlotter --relation "floor(sin(x+sin(y+sin(x))))=floor(cos(y+cos(x+cos(y))))" --output "./output.png" --width 1000 --height 1000 --xmin -10 --xmax 10 --ymin -10 --ymax 10 --drawColor "#E8AC4A"
```

![Pattern](./img/Pattern.png)

## Relation Explorer

Relation Explorer is the graphical version of this project, offering rich interactive plotting features, and is available on [Microsoft Store](https://apps.microsoft.com/detail/9NH7XQTS2QKH).
