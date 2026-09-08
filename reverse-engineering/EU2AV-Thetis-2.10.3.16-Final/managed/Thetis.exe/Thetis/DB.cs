using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Thetis;

internal class DB
{
	public static DataSet ds;

	private static string _file_name = "";

	private static string _version_nr = "";

	private static string _version_str = "";

	private static bool _merged = false;

	private static string _default_settings = "H4sIAAAAAAAEAO1dd3wcRbK2ZBhMzjmJnCztzGwmW5JlDJYtJFkSGLOsdkfSog3yBgUjjpzTkeHIHHBczonjuJxzzjnnnNPr6jA9M/vNij/w3b3fe5a09tb3VXWo6uqeqZVnWduyZcv+xf7Q3/RnO/rHZUMLtbpT6uqpFItOrl6olGtda5yyUy3kutYVavWLrE2bJGWoXi2UJ1d2lGq5SrVYGF/ZMeJUa0zjtFiXSV8rO3oaxXqj6pxWdhr1ara4smOgMV4s5M5xFoYr0075tPFkMhvPxRNWOhpzzFR68+blrA9GpsDs17bP1ApbnRWZWWHVWLZsxYod2xn+BCM9zX72bOfdZgqktXduanpoqjLXM3q+U61E+rLFmmMwcNmuM/XxntGhGcfJR+y4sT2JGvme0YFCPTcVSZimwUwv21eqD4/1jPZVnS3SwA4M2oNBPaNrs6XxQi4yXG04xgrZHjNbyDv1StmR9B0ZsCcH+kbZKJ2q4O/ExPtVs/neQm2mmF04v1Ipxealys7NmK2wXahjfsyaFyZ3bVYz41JtN4btxUbtwSJW3DR2p95p+UC2HImz4e9BdNbpwTF7oOpkSzPSzJ6u3PLJ92LynZm8d0wK9qYpLTfyA3PVYac0Y0dMYx8m2kWLmGRfmknW4b5Cse5ULam6H0lZl4R0aKowUWfc/Zl0H1c6WsjXpyKdaZv19QCaedfKSLZqS0MHBgHVwkG+dtNSerBPmpLSQ3xSZfpQnzQppYf5pFEpPdwnTUhph08ak9IjmHR3VxoXfj2SfKSnyZTco2jO2YwwHw32RVKmcbQMwb5GsdjbmCk6KmaOIauCObSl4RRZjJvGsZLtEQr2cTK+GdDfqKswPp5sCOn6bjUNJzDhblI4KGUnemTdUnaSVl61vk8KV3JhpTReIfGanki/kzc6NbN77XrJ7JJdXT3Pxl/OFgdWSSBCIcWA/g2KavLOT5/bKOSmB52clFq6T0Oqn7Zc4X2jPauGN3YvzGRrNQlFKdZ4z4YbZae/knciAjdiMqSYIX/QxoOAmqKEXJQu4AnQZFBJhUwKKKmW0kElFTsnBwEVlqfQovUCMq5ODSqo2DxNK9DgV/VL+el++bqhbmHoDG2I5EOuwpl+oGd0nQRW+YHeIRUp3X5gowv0BE1tlECvH+jrV6GwmrzoaWPtGqXSF2h9UPV3TZOK6vBZDNlfRJBKlU52WoJrGXiYimQJ86hh6TSbz86wqTXOJpf6DKwaWSP1z6FkybAxe1hF5zqKbLFkRU42+rVkTbZQZit4PUU1l1hcYrFkuEHKmNJQgyGkOSB3vMExk/Ns0zhXZgRG62ciwRuUC59057IqtQ/Jjq8uZ8eLTj/bvguDKssP84XC0n7TlGxkyIF8SoLzMZEtFo0RuaKbpmJUrn+xKqVwTC3zjcNScp7cb9ysc77U6x0a0Olpk9TTmeQCpafCarOkDLtp6ULpjJFVPcpORm7hI30bhmaKhboUX8TEOzTyg2uHmTOy0hC9E/C4lIy5kpw2rVZzXtgY4zYcNyPKzdXMdxsTTLgTcxWlIvLSpHxPSbOPKU2524DF3jPfFni7ZEUl1YuZZEfy9uggWZhmb1eQBt81ivIdU46aRom9O0CFcr/DIpc7bqgwyfKuUVanIpoF6Ts5kIqbMz0OH67MRIwZvnQCSHelXmdHEGOLntrudZWcCp+qStjzq8pqumsyPMkLC2WV2uta6tFvkH59vs6kq9YN9UZMtjRmKeakrJ/Jkl2maREwJxsjYHhMpLR5Go4k87Ofy16QkUY9HlYLYatur1u1d4lur1u0R/oMWNS2u6VtiVwqB+PfkF+kQmtMBeRlsscDlTl2nhSyy+XRwZexr5CnDJWuhfRKj1Qn66s8Up2pr/ZIdZq+xiPVOfpaTxcoQfOZvM5D1cn5enmmCWTmG7ytuWn5Rj9Zde0mj3hoYHWPFN8sE6U+6tzCp1UGdXe2nI/EzJJxq2yLBGtWq47d5pGOjo5I6YtpwqVU7ZK3y9GSzD2U3eEVKnfd6RWqE/ldXqHaqu/2CG1l8x6PMKqE93p6FDPFVN/nISYU8X7PgCxX+hIPNaWED8j9Xc6IpY4jDwbkalwPBeQqqT0ckCv7j0iHSbk6eD/qF6uT92N+sZqix/1i5Y2X+sVqlp/wi9VZ6Um/WA30Kb9YjfNlfrEa5tN+sRrly90UyvPn8BjPoH1z+Q525WO8ws2GHB0c8+bXVzLwYJaKy/lqpeTks6vLOYZWh4rskrJKqftVfDPmJ9OxgWplolB0Ir3ORJbtysarqTuN/PCYOMytq8zxE8FrKA1p8VmFySmW6BnwWpnI+gs5fs7nIfQ6ubuMbBiLdMZM4/Vy6+gZ6B2M2MYb5Fumw4wbb5T7v7hKFg0IO2+SW93w2Opz5by8WYoGtegtMrNx61zvrVJC7QvO25aJS871lULNWZN1L0je7k7zqv7myXhG7op9/bKr75AbTR8j8zkQVp6VGZfENGNC+k6yTVdc/b3ObCFL9zzs6bO2SvQ5V6dnuGdoSErfxftJ/VFyNvtdpvFuedShFoYKJX5Zxof6HlrBDdbGhomJmsOucbss473uoPrAoN5HsePvVpy6xc29X5782OFieEyeED/gOYVNNtv7oFYZVCofkomBHSfIq8L2h2UOoeunKXeWPkKdLWZr9QyDCpXMTLVSr+Qqxcj6StkxPkqx7EGnstX8XLbqRDaWp8uVubLxMWqpXM3UHLqt5OQj5qJpfJwJD+IHtExuyslN1+rk8Y1l/oYdZT7B8EPV1SWmfJIPemr63KFzvIQeCX+KwYfrPRbb+DQjHem7OMa8z9Agq/NWJi+ONplctpipCH922syhn6XOEKFEq90Lm13plPE5oW+H6n9e6Nsh+l+g6WJHiX6nVKku8KveiGVHY13xRDKVNo0vUtRvIXGmlp11MiVKNqbxJYpJj3hCrFzb+DId8Ki7NTlqMeANfX3GV/iaYNBECaBfZehhXsX6VNWpTVWK+UwtV60Ui6zVrzHOEQELgPZ1RjuaaLOVQs5pxfyG6K0N+vNN0Vsb9/Zbord2K+PfFr21l+jtd0Rv7aV7+12+tos06XV2llcul4vpe7QW/adN4/s0vh6nXK86JG045dyChH7AoEM8xjwxJA3+kDKp94hp/IgSMDXBrgy50DR+TPuJaIHuAngasYyfULbJF/i91vpCZpLlhww7QJQiFstqP6XAC4BVW+M/o8FqfGYqW3MESjnx59RsQDslNX9BEd9sWcG/9PdKGE5Ju78CdlWPfo3tKvg3yK7q72+B3ZhU/B22q+DfI7sxafcPwG5UKv4R21Xwn5DdqLT7Z2DXlop/wXYV/Fdk15Z2/wbsWkmh+HdsV8H/QHYJJLv/RHbjQvFfIXYlvKwN2Y0Lu21twK4tFNvbsF0JL4d2bWF3O2RXTuD2IXYlbEC7cn53YOC+wfgVeivafAvKDV+B7ujvkIxeYXQn0Nm5uVmutzPurIJ3QZ0lkOzuCuxOOmWuuBu2q+DdkV0Cye4ebc15Z362XuWae7bBvOPie7WBvMNRMr03Q4+YY7tAle5CZabYOdCTqXl66kyZxj5+WrEy18SybNPYl9E6Qq2lpLH9/Cy/sZSytX9LW6pjB7Sy5fbrwJa2YtLWQa1sxZStg1vaikpbh7SyxUlW1DQObWnLlrYOa2XLVrYOb2mL8grZ6mhli5PI1hGtbcWFrSNb2opLW0e1tmULW0e3tEUki117HdPalpyvY1vaMqWt4xjr8PD4EqaO95MC4SUtndCyV5QcyNSJrXrFSTRbJ7W0RQmBbK1sZYuTKFI7W65tngLIWFertS1YNMoIox1Dh6qls4XZTA3LGFabPCwumTXsZmZI5oguaVN1NLaUTbef8SVtqiySWMqmm0mSS9pU2SS1lE03o6SXtKmyyslL2XQzyylL2lTZ5dSlbLoZ5rSlbcosc/qSNlWmOWNpmzLbnLmkTZVxVi1tU85n95I2VebpYcyjWsenMNnbTMQZaPWSvVRZqG+pXrqZaM2SNlU2Omspm25GWrtkDnGz0tlL5RCdmc4RRytxu2CyWshnStl5mWMYvE4ckLywum7j6v3igBRQT0nt9c3G5aUZV94AlVXTA1DZbflcqByTyoNQOaaUh6ByVCoPQ+WoUt4IlW2pPAKVbaU8CpX50mboGFQWC5/B52HluFA+HyvHpfImrGwL5QuwslrKm7GyHPOFWFmNOSOO4EE/C92Lmi2LyxSumoXt8nXG0HHYrliFDM5BZb6gGJqHymK5MdiBC0MsHAZPwIWh19Ukww/13oprXltTbfK2UYv1VWCcQ6AZtcYuxg1519l0qBHVlWKoEbcnpVAjas2VQ424664SakStvZlQI+762xJqRK3BaqgRdx3WQo2otVgPNeKux0a4EbkmZ8ONqHU5F25Ers35cCNqfS6EG5FzsjXciJqTS9oCt4+Da3URt+JZr5eG9kOt2ReF9sNdt5eFGlFr9/JQI+76vSJ0Abpr+MrQBajX8VXiGGHJm9q1TeZm9h2Jd5nTJ3d0xi3TXOzopMLY1YBobY7EumJEjMVd4jWAaG+ORLtSRIymXeK1gBglYpQTYy7xOkCMbY7YXWlONF3i9YAYJ2KSiHbKJd4AiAki8sHYejA3AmKSiBYn2i7xJkBMbY5YYtSWHvXNgJgmIp9wS0/4LeKQ459w5pqRbLXD8g/nVkS1BNX2U29rkxUWL5W55+QOxjCNFzN4X1GGcjkUD32WcXubqkdITcsTKbzbFC3GHYDmxgmnUawYdwKaGyWcRpFi3AVobowIGosT425AcyNE0KiGew+gufHBaTRTxr2A5kaHoNEQ7gM0NzYEjT5wez+guZHBaRQdxksAzY0LQaPpfSAYPpYvKvQgHkREHROa+FAwIixfRDyMIoLZ6YsbjzDoSK+mzSPCSlAkp/iypFfjUcBjFiybeAnOo1fjMcBjHbFEKuI8ejUeDw7N5kGREksoxon0arwUEFlYJLp4D6NRHhjs1XgCEOMU27yLdoJPFn3Q/UlATFB086Zt3jS9Gk8BYpIClycOKx4nh7JX42WAmHKTmxXjRPZqPA2IaTdnWTwV0avx8mA2sP2JwzPuVyCqJ3F4qK8MhontC5NXoTCxeZi8OtjxqAiTpoz3mqD/ozxOWAydzAnEowG+NrhUojxOEpwnMi31+HWAxsIkzmmpOKexeX09oLEgiQkat0Yp5g2ARrkjTrSkHSdaMm68EdBYgFiclkgSLW7HjTcBWpJCndN43yg43sxoHQFaioZALN5mnLX5FsCiyIgTy4oSK5U03tomC8Xe2fWEhZ6RtyGmJyo08+2MeXiQyYOik68ZejWeQaER5aHxjuA0xDyhoZPes8EBxnRgiLinJffOYGdiOix4HFOafQ6QVFBEaVxscMa7AEmFREymNePdgKQCIkaOZkFhvAeQVDjEyYEsJIz3ApIKBp4oWEAY72uTHxnwkGQoxKk1Fg7G+wFHBgKLAMaxosYHgoss5gsDdw4+iHg6CFzeh8Q51M/jIZCQid34MAqAGA+Aj7Spj0lI7TgPAJlUPgpQy0U/BlCdkD4O0KiLfgKgMRf9JEDjLvopgCZc9NMATbroZwCactHPAjTtop8LZuC48JyEP49gPVtfQLCeri/SPYygj9iY129Yv9r4UjAhJLiXbJHATVMtQdP4cjBqEmKr56fgtClXoWl8BfBscHT4KuBFwdHha4AXA0eHrwf3ogT3bFoeHeLu+fQbgJhAZ4xvAiLzd1IQo9wivRrfAsQUHUZMfRXDT6nfBsS0e0kmLk74AfQ7wd07Edjotc3vIqp3o9fU7wUDJeELlO+jxZzgi/kHwTyW5GES5R1X5n8YTPlJHiF2V5yRbMoqdpKllR8Bmk00sqWn4MeARvHBrSW5NcYzfhIMj6QID7XPqFPbT4G5uNxoLH1W/xmgJeROIw9itEP8HNDoDMhpPIFalNl/AWgpuUdYMdokLNrlfxncBJM8LJJxYiVsYkVTxq+CSzXpvz7Qg/g1YnouEDTzN8GISPoi4rcoIpI8In4XTGgpb3r/PUB1wvoDQHWjfwSoTu9/AqhO738GqE7vfwGoTu9/BahO738DqE7vfweoTu//CM5yypfe/4lgPVv/QrCermXtIL2nZHpvaw8knLRn3XZGkxad5Nir0Q6I7trtjMYSREwnTGN5eyC20p7V2xm1uUWL/WVsB5juAu6009wki2/T2B4w3TXcaYteWtRNA3RTrWJ2MSeIKUbcARDVOmaJVhDTjLgCENVK7rSjCXFlwjq5IyCqtdxp2yk6stts7Rs7tQcyUtpdzYwXS3Fe0jZ2Zrxjg1PuvQvkGc4uiOu9DeTh7toeiJe0L152awdrOs3X9O7tga3EMsNOA3sEZ8My8XFgT0RE54G9EBEdCPZGRHQi2CcYVpYZciTYFzHhmWA/xISHgv0RE54KDkBMeCw4MBgGlhl6LjgIcvHB4GDGPaiJq2PmEIbv13SnyORBc2gw3CzLuxMchmCd3A5HsG64A8F6MzgCwXo3OBLBejs4CsF6PzgawXpDOAbBekc4FsF6SziuacYt355wPMT1vJ0AcT1xJ7a7v5Lg8Zglt4WTmjpne322EsG67U4E66a7EKx9FkGw9pmJYO0zC8HaZzaCtc+iCNY+iyFY+yzeNOe2z2cJiOt5S0JcT1wK+syWPks3dS7q9dnJCNZtn4Jg3fSpCNY+Ow3B2menI1j77AwEa5+diWDts1UI1j7rRrD2WU/TnEd9PuuFuJ631RDXE9cHfRaVPlvTLj/Jo7TDSnBnASIswa0FRFiCOxsQYQnuHECEJbh1gAhLcP2ACEtw6wERluA2ACIswQ0AIizBndsuP2fknXBcghtEVFyCG2qXv6TkpepgGfYcvRRnk9hFN7arX7JRcQZLcCOABkpwo4AGSnBjgAZKcOcBGijBnQ9ooAS3CdBACe4CQAMluM2ABkpwFwIaKMFlguETVoK7CBFRCS4bjAj/Nj2OIkKU4HLiMkJrhpTg8oCHSnAO4KES3ERwaGEluElAhCW4KUCEJbgCIMIS3MWACEtw04AIS3BFQIQluFIwG4SX4MqIiktwlWCY+E8GMyhMRAluS7DjYSW4atD/ISW4WnCp4BJcHdBACa4BaKAENwtooAQ3B2igBDcPaKAEt9DOb7v5aM0luK2A1VyCu6RdfhjWO7uwBLeImLAEd2k7v+nqZwZLcC9CoSFKcJcFpwGX4C4PDhCW4K4IdgaV4K4EpKYS3FWA1FSCuxqQmkpw1wBSUwnuWkBqKsFd1y4/T+UhBUtw1wNOsAR3Q3CRhZTgbkQ8UIK7qV1+FMzL85fgbkYBIEpwt7Sr3/2V2r4S3K0A1Qff2wDq+TARQPXlwu0A1VcLdwBUXyzcCVB9rXAXQPWlwt0A1VcK9wBUXyjcG8zA/hLcfQjWs3U/gvV0vaRd36N1faRKcA8EE0JoCe7BYNSElOAeAjx0y+1hwEN33B4BPHTD7dHgXhRWgnsMEOHttscBEd5teykgwpttTwAivNf2ZHD3Di/BPYWo+E7by4KB4i/BPY0WsyjBvTyYx1AJ7hXBlI9LcK8ENFCCexWggRLcq4PhEVKCew0wB0pwrwU0UIJ7HaCBEtzrAQ2U4N4Q3ARhCe6NwaUaWoJ7E2LCEtybgxHhL8G9BUWEKMG9NZjQfCW4twFUJ6y3A1Q3+gxAdXp/B0B1en8WoDq9vxOgOr0/B1Cd3t8FUJ3e3w1Qnd7fE5xlfwnuvQjWs/U+BOvpej9K76oE94FgwgkrwX0QEGEJ7kPB2AotwX0YMHEJ7iOAiUtwHwXdhCW4jwEiLMF9HBBhCe4TgAhLcJ8MZqSQEtyn2nmNxD/lISW4TyNuSAnuM8F48ZfgPovWtCjBfS64lYSX4D7fdA8ipAT3BURE54EvIiI6EHwJEdGJ4MvBsAotwX0FMeGZ4KuICQ8FX0NMeCr4OmLCY8E3gmHQogT3TcjFB4NvtfMbzQGujplvt+sSnL5TJG4efqfpJpOvBPddBOvk9j0Eez4VhGC9GfwAwXo3+CGC9XbwIwTr/eDHCNYbwk8QrHeEnyJYbwk/a5pxfwnu5xDX8/YLiOuJ+2W7Lg1oj6kS3K+aOucrwf0awbrt3yDY87kdBGuf/Q7B2me/R7D22R8QrH32RwRrn/0Jwdpnf0aw9tlfmubcX4L7K8T1vP0N4nri/g59pkpw/2jqnK8E908Eez7Hg2DP53iWA1j7rA3B2mftCNY+W45g7bPtEKx9tj2Ctc8MBGuf7bA8OOf+EtwKiOt52xHieuJ2Wo58pkpwOzP0BKrNFZ1Zp5ip0//dTD5jJ68uyuq22UVJuNOy6K2xC6JbofRdEd0Ope+G6NFQ+u6IHgul74Ho8VD6noieCKXvhejJUPreiJ4Kpe+D6OlQ+r6MfmKTm8Lduh/kh/t1f8gPd+wBkB/u2QMhP9y1B0F+uG8Phvxw5x4C+eHePRTyw917GOSH+/dwxLfD/dsB+eH+PQLyw/17JOSH+/coyA/379GQH+7fYyA/3L/HQn64f4+D/HD/Hg/54f49AfGj4f49EfLD/XsS5If7dyXkh/u3E/LD/dsF+eH+jUB+uH9NyA/3rwX54f61IT/cv1HEj4X7Nwb54f6NC74dtqt3BvgJxLfC+UnEt8P5KcSPhvPTiB8L55+M+PFw/imInwjnn4r4yXD+aYifCuefjvjpcP4ZjH9Sk79aOPhMqNDCw6ugQgsXd0OFFj7ugQotnNwLFVp4eTVUaOHmPqjQws9roEILR58FFVp4ei1SsFt4+myo0MLT50CFFp5eBxVaeLofKrTw9Hqo0MLTG6BCC08PQIUWnj4XKrTw9CBUaOHpIaQQbeHpYajQwtMboUILT49AhRaeHoUKLTw9BhVaePo8qNDC0+dDhRae3gQVWnj6AqjQwtObkUKshacvhAotPJ1hChN0XpjhzwfKjC9kxulxJtbiC/5lXCTasv8dbWVZWxUaV63uzGSy9bpTbmTrlarbqLn4gn8Z46JR+9/aaI41el9bfT600ai1+J/7NvKseze3zdBjhbTDWQDG/2PfhsO6dFsb/Sf3A75u/ee6RL2aYL16pq1YKBXqA02z9b/gx5hkA3hODGC4aW7/Czr4vAYxxQbxbNtEKcMWlHgshhpCpjS1lRZwl/W8fp4faxv8GAUe3pT7spM5t/uRlLmYjMtv9m/6jtN32uTf8UXLMtWbF/7buFj2yv6v6tU061VeTlVJZ8zFF/zLKIqW7G3fUom1VNL/YT2NrupMbNPdtsyaHKeGttbH89l6NpOb2CZ7XSXYzkxtm7QzE2xn67ZpZwttju3ehgpl9TDIxf+Tr0ZVTL29rUOpFmxnG4VSPdjONgqlhgwl+/9DSYXSLJuSq9mW0/SkK8qF/BFi/JFXi8/jJdaVilmJWAz8ozPWFUsm1F+u2KdmzLGuLLCeND+US/eFns61+Dxe4l12PO17pbZNM+7+LaUu2Zinyu5MtuwU6cmCZ/V1jRRqBXZpKJ7FtkB1YRcd8cBiHrf68DWr1wfwSxi+Z6GWqxSL2Zmak5fiRSbevVBz5plqnkl5W5eS0N2c1ONIl8tnnskLQzEz4tNadLVqXLZc/vKon8CuawV+OTbAfyuLE67AhKjbwpWYEKNPz3HCVZgQp9+34ISrMSFBv/zACddgQtLt5LWYkHI7eR0mpN1OXs8Ie81NOXTpT8/wKrCJn4/Yxg1MHgvKKfL409vsxdAv40amuVuhPFHpzlYzE8XCTMQ0buL+lrLxRr1eKVtMfHOz2I5Yxi1MfKgS1+gxv7QYq/SsQfpUJHX7VtLMNuoVeqBatZ6ZqFRLtYhxG8XK7ERlPJOvzagHzb2YCXflQvEBBTa625lov8Exiz/+ciCbzxfKk4PcvmncsZw/ALlcqxSdTL0yw6YyZdy5nD+9VQiLzkSdS+9azh+3KKRzhXx9ijkmGjPu9sqnnMLkVD2SiKWMe7ymxZPgTONeJtyZOaUxw1szjfuYYBch4C2Zxv0UzGvLuWIj74wyT1TmuivVPFsQ6kGqtFjVc635c5HZzA041RwL9rjxAA22t1Ga6cnOZEYLVWdoKludHsiy3hoPMmxvha0t1+n//c85zAcPMeBABax3JtnkzDobysUFsSgfZvARCu4pOtlqX6XIerShPOhwjwjaI4x2zJCzZV1lMkMPbRzNVstsqjeUmy0+Sj3pGdjIeUMLtbpTEsBjBNATAvPsYJqpkIIY9ePkk1KlzM6qLDkW1TNYmTQyQsmiUo4MTzn1Qq1jluU1syvaZUU75hOxjuPNKPNTxE6c0JF3ZqNly3iCKe0jldY3SuMUJFzFeJKCh5LYsDNfX9sb2ThkPEXeHaiyVdGdzU03ZqQXdr1g5/8B/JLCXbyEAAA=";

	public static string FileName
	{
		get
		{
			return _file_name;
		}
		set
		{
			_file_name = value;
		}
	}

	public static string VersionNumber
	{
		get
		{
			return _version_nr;
		}
		set
		{
			_version_nr = value;
		}
	}

	public static string VersionString
	{
		get
		{
			return _version_str;
		}
		set
		{
			_version_str = value;
		}
	}

	public static bool Merged => _merged;

	private static void VerifyTables()
	{
		if (!ds.Tables.Contains("BandText"))
		{
			AddBandTextTable();
		}
		if (!ds.Tables.Contains("BandStack2Entries"))
		{
			AddBandStack2EntriesTable("BandStack2Entries");
		}
		if (!ds.Tables.Contains("BandStack2Filters"))
		{
			AddBandStack2FiltersTable("BandStack2Filters");
		}
		if (!ds.Tables.Contains("BandStack2FilterFrequencies"))
		{
			AddBandStack2FilterFrequenciesTable("BandStack2FilterFrequencies");
		}
		if (!ds.Tables.Contains("BandStack2FilterModes"))
		{
			AddBandStack2FilterModesTable("BandStack2FilterModes");
		}
		if (!ds.Tables.Contains("BandStack2FilterSubModes"))
		{
			AddBandStack2FilterSubModesTable("BandStack2FilterSubModes");
		}
		if (!ds.Tables.Contains("BandStack2FilterBands"))
		{
			AddBandStack2FilterBandsTable("BandStack2FilterBands");
		}
		if (!ds.Tables.Contains("BandStack2HiddenEntries"))
		{
			AddBandStack2HiddenEntriesTable("BandStack2HiddenEntries");
		}
		if (!ds.Tables.Contains("Memory"))
		{
			AddMemoryTable();
		}
		if (!ds.Tables.Contains("GroupList"))
		{
			AddGroupListTable();
		}
		if (!ds.Tables.Contains("TXProfile"))
		{
			AddTXProfileTable("TXProfile");
		}
		if (!ds.Tables.Contains("TXProfileDef"))
		{
			AddTXProfileTable("TXProfileDef", bIndcludeExtraProfiles: true);
		}
		VerifyTXProfileColumns();
		WriteDB();
	}

	private static void VerifyTXProfileColumns()
	{
		string[] array = new string[2] { "TXProfile", "TXProfileDef" };
		foreach (string name in array)
		{
			if (ds.Tables.Contains(name))
			{
				DataTable dataTable = ds.Tables[name];
				if (!dataTable.Columns.Contains("CFCPhaseRotatorAuto"))
				{
					dataTable.Columns.Add("CFCPhaseRotatorAuto", typeof(bool));
				}
			}
		}
	}

	public static bool IsDatabaseCompatible(out string reason)
	{
		reason = "";
		if (ds == null)
		{
			reason = "Dataset is null.";
			return false;
		}
		string[] array = new string[3] { "TXProfile", "TXProfileDef", "State" };
		foreach (string text in array)
		{
			if (!ds.Tables.Contains(text))
			{
				reason = "Required table '" + text + "' is missing.";
				return false;
			}
		}
		DataTable dataTable = ds.Tables["TXProfile"];
		DataTable dataTable2 = ds.Tables["TXProfileDef"];
		foreach (DataColumn column in dataTable2.Columns)
		{
			if (!dataTable.Columns.Contains(column.ColumnName))
			{
				reason = "TXProfile table is missing column '" + column.ColumnName + "'.";
				return false;
			}
		}
		if (dataTable2.Select("Name = 'Default'").Length == 0)
		{
			reason = "TXProfileDef table has no 'Default' row.";
			return false;
		}
		foreach (DataRow row in dataTable.Rows)
		{
			if (row.RowState == DataRowState.Deleted)
			{
				continue;
			}
			string text2 = (row.Table.Columns.Contains("Name") ? Convert.ToString(row["Name"]) : "<unknown>");
			foreach (DataColumn column2 in dataTable2.Columns)
			{
				if (row.IsNull(column2.ColumnName))
				{
					reason = "TXProfile '" + text2 + "' has no value for '" + column2.ColumnName + "'.";
					return false;
				}
			}
		}
		Dictionary<string, string> varsDictionary = GetVarsDictionary("State");
		if (!varsDictionary.ContainsKey("VersionNumber"))
		{
			reason = "State table is missing VersionNumber.";
			return false;
		}
		if (!varsDictionary.ContainsKey("Version"))
		{
			reason = "State table is missing Version.";
			return false;
		}
		return true;
	}

	private static void AddBandStack2HiddenEntriesTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("FilterGUID", typeof(string));
		dataTable.Columns.Add("EntryGUID", typeof(int));
	}

	private static void AddBandStack2FilterBandsTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("FilterGUID", typeof(string));
		dataTable.Columns.Add("Band", typeof(int));
	}

	private static void AddBandStack2FilterSubModesTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("FilterGUID", typeof(string));
		dataTable.Columns.Add("SubMode", typeof(int));
	}

	private static void AddBandStack2FilterModesTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("FilterGUID", typeof(string));
		dataTable.Columns.Add("Mode", typeof(int));
	}

	private static void AddBandStack2FilterFrequenciesTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("FilterGUID", typeof(string));
		dataTable.Columns.Add("Low", typeof(double));
		dataTable.Columns.Add("High", typeof(double));
		dataTable.Columns.Add("LowOnly", typeof(bool));
		dataTable.Columns.Add("Band", typeof(int));
		dataTable.Columns.Add("BandType", typeof(int));
		dataTable.Columns.Add("Region", typeof(int));
	}

	private static void AddBandStack2FiltersTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("GUID", typeof(string));
		dataTable.Columns.Add("FilterName", typeof(string));
		dataTable.Columns.Add("FilterDescription", typeof(string));
		dataTable.Columns.Add("FilterOnFrequencies", typeof(bool));
		dataTable.Columns.Add("FilterOnBands", typeof(bool));
		dataTable.Columns.Add("FilterOnModes", typeof(bool));
		dataTable.Columns.Add("FilterOnSubModes", typeof(bool));
		dataTable.Columns.Add("UserDefined", typeof(bool));
		dataTable.Columns.Add("FilterReturnMode", typeof(int));
		dataTable.Columns.Add("SpecificReturnGUID", typeof(string));
		dataTable.Columns.Add("CurrentSelectedIndex", typeof(int));
		dataTable.Columns.Add("CurrentSelectedGUID", typeof(string));
		dataTable.Columns.Add("LastVisitedGUID", typeof(string));
		dataTable.Columns.Add("LastVisitedDescription", typeof(string));
		dataTable.Columns.Add("LastVisitedLocked", typeof(bool));
		dataTable.Columns.Add("LastVisitedFrequency", typeof(double));
		dataTable.Columns.Add("LastVisitedCentreFrequency", typeof(double));
		dataTable.Columns.Add("LastVisitedBand", typeof(int));
		dataTable.Columns.Add("LastVisitedMode", typeof(int));
		dataTable.Columns.Add("LastVisitedSubMode", typeof(int));
		dataTable.Columns.Add("LastVisitedCTUNEnabled", typeof(bool));
		dataTable.Columns.Add("LastVisitedFilter", typeof(int));
		dataTable.Columns.Add("LastVisitedZoomFactor", typeof(double));
		dataTable.Columns.Add("LastVisitedZoomSlider", typeof(int));
	}

	private static void AddBandStack2EntriesTable(string sTableName)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("GUID", typeof(string));
		dataTable.Columns.Add("Description", typeof(string));
		dataTable.Columns.Add("Locked", typeof(bool));
		dataTable.Columns.Add("Frequency", typeof(double));
		dataTable.Columns.Add("CentreFrequency", typeof(double));
		dataTable.Columns.Add("Band", typeof(int));
		dataTable.Columns.Add("Mode", typeof(int));
		dataTable.Columns.Add("SubMode", typeof(int));
		dataTable.Columns.Add("CTUNEnabled", typeof(bool));
		dataTable.Columns.Add("Filter", typeof(int));
		dataTable.Columns.Add("ZoomFactor", typeof(double));
		dataTable.Columns.Add("ZoomSlider", typeof(int));
		dataTable.Columns.Add("PowerLevel", typeof(double));
		dataTable.Columns.Add("AGCLevel", typeof(double));
		dataTable.Columns.Add("FilterLow", typeof(int));
		dataTable.Columns.Add("FilterHigh", typeof(int));
	}

	public static void SaveBandStack2Filter(BandStackFilter bsf)
	{
		if (_merged)
		{
			return;
		}
		string filterExpression = "GUID = '" + bsf.GUID + "'";
		DataRow[] array = ds.Tables["BandStack2Filters"].Select(filterExpression);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Delete();
		}
		filterExpression = "FilterGUID = '" + bsf.GUID + "'";
		array = ds.Tables["BandStack2FilterFrequencies"].Select(filterExpression);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Delete();
		}
		array = ds.Tables["BandStack2FilterModes"].Select(filterExpression);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Delete();
		}
		array = ds.Tables["BandStack2FilterSubModes"].Select(filterExpression);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Delete();
		}
		array = ds.Tables["BandStack2FilterBands"].Select(filterExpression);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Delete();
		}
		DataTable dataTable = ds.Tables["BandStack2Filters"];
		DataRow dataRow = dataTable.NewRow();
		dataRow["GUID"] = bsf.GUID;
		dataRow["FilterName"] = bsf.FilterName;
		dataRow["FilterDescription"] = bsf.FilterDescription;
		dataRow["FilterOnFrequencies"] = bsf.FilterOnFrequencies;
		dataRow["FilterOnBands"] = bsf.FilterOnBands;
		dataRow["FilterOnModes"] = bsf.FilterOnModes;
		dataRow["FilterOnSubModes"] = bsf.FilterOnSubModes;
		dataRow["UserDefined"] = bsf.UserDefined;
		dataRow["FilterReturnMode"] = bsf.ReturnMode;
		dataRow["SpecificReturnGUID"] = bsf.ReturnGUID;
		dataRow["CurrentSelectedIndex"] = bsf.IndexOfCurrent;
		dataRow["CurrentSelectedGUID"] = bsf.GuidOfCurrent;
		dataRow["LastVisitedGUID"] = bsf.LastVisited.GUID;
		dataRow["LastVisitedDescription"] = bsf.LastVisited.Description;
		dataRow["LastVisitedLocked"] = bsf.LastVisited.Locked;
		dataRow["LastVisitedFrequency"] = bsf.LastVisited.Frequency;
		dataRow["LastVisitedCentreFrequency"] = bsf.LastVisited.CentreFrequency;
		dataRow["LastVisitedBand"] = (int)bsf.LastVisited.Band;
		dataRow["LastVisitedMode"] = (int)bsf.LastVisited.Mode;
		dataRow["LastVisitedSubMode"] = (int)bsf.LastVisited.SubMode;
		dataRow["LastVisitedCTUNEnabled"] = bsf.LastVisited.CTUNEnabled;
		dataRow["LastVisitedFilter"] = (int)bsf.LastVisited.Filter;
		dataRow["LastVisitedZoomFactor"] = bsf.LastVisited.ZoomFactor;
		dataRow["LastVisitedZoomSlider"] = bsf.LastVisited.ZoomSlider;
		dataTable.Rows.Add(dataRow);
		dataTable = ds.Tables["BandStack2FilterFrequencies"];
		foreach (BandFrequencyData item in bsf.FrequenciesToFilterOn)
		{
			dataRow = dataTable.NewRow();
			dataRow["FilterGUID"] = bsf.GUID;
			dataRow["Low"] = item.low;
			dataRow["High"] = item.high;
			dataRow["LowOnly"] = item.lowOnly;
			dataRow["Band"] = (int)item.band;
			dataRow["BandType"] = (int)item.bandType;
			dataRow["Region"] = (int)item.region;
			dataTable.Rows.Add(dataRow);
		}
		dataTable = ds.Tables["BandStack2FilterModes"];
		foreach (DSPMode item2 in bsf.ModesToFilterOn)
		{
			dataRow = dataTable.NewRow();
			dataRow["FilterGUID"] = bsf.GUID;
			dataRow["Mode"] = (int)item2;
			dataTable.Rows.Add(dataRow);
		}
		dataTable = ds.Tables["BandStack2FilterSubModes"];
		foreach (DSPSubMode item3 in bsf.SubModesToFilterOn)
		{
			dataRow = dataTable.NewRow();
			dataRow["FilterGUID"] = bsf.GUID;
			dataRow["SubMode"] = (int)item3;
			dataTable.Rows.Add(dataRow);
		}
		dataTable = ds.Tables["BandStack2FilterBands"];
		foreach (Band item4 in bsf.BandsToFilterOn)
		{
			dataRow = dataTable.NewRow();
			dataRow["FilterGUID"] = bsf.GUID;
			dataRow["Band"] = (int)item4;
			dataTable.Rows.Add(dataRow);
		}
	}

	public static void RemoveBandStack2Entry(BandStackEntry bse)
	{
		string filterExpression = "GUID = '" + bse.GUID + "'";
		DataRow[] array = ds.Tables["BandStack2Entries"].Select(filterExpression);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Delete();
		}
		removeReferencesToThisBSEGuid(bse.GUID);
	}

	private static void removeReferencesToThisBSEGuid(string sGUID)
	{
		if (!(sGUID == ""))
		{
			string filterExpression = "SpecificReturnGUID = '" + sGUID + "'";
			DataRow[] array = ds.Tables["BandStack2Filters"].Select(filterExpression);
			for (int i = 0; i < array.Length; i++)
			{
				array[i]["SpecificReturnGUID"] = "";
			}
			filterExpression = "CurrentSelectedGUID = '" + sGUID + "'";
			array = ds.Tables["BandStack2Filters"].Select(filterExpression);
			foreach (DataRow obj in array)
			{
				obj["CurrentSelectedGUID"] = "";
				obj["CurrentSelectedIndex"] = -1;
			}
			filterExpression = "LastVisitedGUID = '" + sGUID + "'";
			array = ds.Tables["BandStack2Filters"].Select(filterExpression);
			foreach (DataRow obj2 in array)
			{
				obj2["LastVisitedGUID"] = "";
				obj2["LastVisitedDescription"] = "";
				obj2["LastVisitedLocked"] = false;
				obj2["LastVisitedFrequency"] = 0;
				obj2["LastVisitedCentreFrequency"] = 0;
				obj2["LastVisitedBand"] = 0;
				obj2["LastVisitedMode"] = 3;
				obj2["LastVisitedSubMode"] = 0;
				obj2["LastVisitedCTUNEnabled"] = false;
				obj2["LastVisitedFilter"] = 12;
				obj2["LastVisitedZoomFactor"] = 0;
				obj2["LastVisitedZoomSlider"] = 0;
			}
		}
	}

	public static void RemoveAllBandStack2Entries()
	{
		DataRow[] array = ds.Tables["BandStack2Entries"].Select();
		foreach (DataRow obj in array)
		{
			removeReferencesToThisBSEGuid(ConvertFromDBVal<string>(obj["GUID"]));
			obj.Delete();
		}
		array = ds.Tables["BandStack2Filters"].Select();
		foreach (DataRow obj2 in array)
		{
			obj2["CurrentSelectedIndex"] = -1;
			obj2["CurrentSelectedGUID"] = "";
			obj2["FilterReturnMode"] = 3;
			obj2["SpecificReturnGUID"] = "";
			obj2["LastVisitedGUID"] = "";
			obj2["LastVisitedDescription"] = "";
			obj2["LastVisitedLocked"] = false;
			obj2["LastVisitedFrequency"] = 0;
			obj2["LastVisitedCentreFrequency"] = 0;
			obj2["LastVisitedBand"] = 0;
			obj2["LastVisitedMode"] = 3;
			obj2["LastVisitedSubMode"] = 0;
			obj2["LastVisitedCTUNEnabled"] = false;
			obj2["LastVisitedFilter"] = 12;
			obj2["LastVisitedZoomFactor"] = 0;
			obj2["LastVisitedZoomSlider"] = 0;
		}
	}

	public static void AddBandStack2Entry(BandStackEntry bse)
	{
		DataTable dataTable = ds.Tables["BandStack2Entries"];
		RemoveBandStack2Entry(bse);
		DataRow dataRow = dataTable.NewRow();
		dataRow["GUID"] = bse.GUID;
		dataRow["Description"] = bse.Description;
		dataRow["Locked"] = bse.Locked;
		dataRow["Frequency"] = Math.Round(bse.Frequency, 6);
		dataRow["CentreFrequency"] = Math.Round(bse.CentreFrequency, 6);
		dataRow["Band"] = (int)bse.Band;
		dataRow["Mode"] = (int)bse.Mode;
		dataRow["SubMode"] = (int)bse.SubMode;
		dataRow["CTUNEnabled"] = bse.CTUNEnabled;
		dataRow["Filter"] = (int)bse.Filter;
		dataRow["ZoomFactor"] = bse.ZoomFactor;
		dataRow["ZoomSlider"] = bse.ZoomSlider;
		dataTable.Rows.Add(dataRow);
	}

	public static void AddBandStack2Entry(List<BandStackEntry> bseList)
	{
		foreach (BandStackEntry bse in bseList)
		{
			AddBandStack2Entry(bse);
		}
	}

	public static List<BandStackEntry> GetBandStack2Entries()
	{
		List<BandStackEntry> list = new List<BandStackEntry>();
		DataRow[] array = ds.Tables["BandStack2Entries"].Select();
		foreach (DataRow dataRow in array)
		{
			BandStackEntry bandStackEntry = new BandStackEntry();
			bandStackEntry.GUID = ConvertFromDBVal<string>(dataRow["GUID"]);
			bandStackEntry.Description = ConvertFromDBVal<string>(dataRow["Description"]);
			bandStackEntry.Locked = (bool)dataRow["Locked"];
			bandStackEntry.Frequency = Math.Round((double)dataRow["Frequency"], 6);
			bandStackEntry.CentreFrequency = Math.Round((double)dataRow["CentreFrequency"], 6);
			bandStackEntry.Band = (Band)dataRow["Band"];
			bandStackEntry.Mode = (DSPMode)dataRow["Mode"];
			bandStackEntry.SubMode = (DSPSubMode)dataRow["SubMode"];
			bandStackEntry.CTUNEnabled = (bool)dataRow["CTUNEnabled"];
			bandStackEntry.Filter = (Filter)dataRow["Filter"];
			bandStackEntry.ZoomFactor = (double)dataRow["ZoomFactor"];
			bandStackEntry.ZoomSlider = (int)dataRow["ZoomSlider"];
			list.Add(bandStackEntry);
		}
		return list;
	}

	public static Dictionary<string, BandStackFilter> GetBandStack2Filters()
	{
		Dictionary<string, BandStackFilter> dictionary = new Dictionary<string, BandStackFilter>();
		DataRow[] array = ds.Tables["BandStack2Filters"].Select();
		foreach (DataRow dataRow in array)
		{
			BandStackFilter bandStackFilter = new BandStackFilter();
			bandStackFilter.GUID = ConvertFromDBVal<string>(dataRow["GUID"]);
			bandStackFilter.FilterName = ConvertFromDBVal<string>(dataRow["FilterName"]);
			bandStackFilter.FilterDescription = ConvertFromDBVal<string>(dataRow["FilterDescription"]);
			bandStackFilter.FilterOnFrequencies = (bool)dataRow["FilterOnFrequencies"];
			bandStackFilter.FilterOnBands = (bool)dataRow["FilterOnBands"];
			bandStackFilter.FilterOnModes = (bool)dataRow["FilterOnModes"];
			bandStackFilter.FilterOnSubModes = (bool)dataRow["FilterOnSubModes"];
			bandStackFilter.UserDefined = (bool)dataRow["UserDefined"];
			bandStackFilter.ReturnMode = (BandStackFilter.FilterReturnMode)dataRow["FilterReturnMode"];
			bandStackFilter.ReturnGUID = ConvertFromDBVal<string>(dataRow["SpecificReturnGUID"]);
			bandStackFilter.IndexOfCurrent = (int)dataRow["CurrentSelectedIndex"];
			bandStackFilter.LastVisited.GUID = ConvertFromDBVal<string>(dataRow["LastVisitedGUID"]);
			bandStackFilter.LastVisited.Description = ConvertFromDBVal<string>(dataRow["LastVisitedDescription"]);
			bandStackFilter.LastVisited.Locked = (bool)dataRow["LastVisitedLocked"];
			bandStackFilter.LastVisited.Frequency = (double)dataRow["LastVisitedFrequency"];
			bandStackFilter.LastVisited.CentreFrequency = (double)dataRow["LastVisitedCentreFrequency"];
			bandStackFilter.LastVisited.Band = (Band)dataRow["LastVisitedBand"];
			bandStackFilter.LastVisited.Mode = (DSPMode)dataRow["LastVisitedMode"];
			bandStackFilter.LastVisited.SubMode = (DSPSubMode)dataRow["LastVisitedSubMode"];
			bandStackFilter.LastVisited.CTUNEnabled = (bool)dataRow["LastVisitedCTUNEnabled"];
			bandStackFilter.LastVisited.Filter = (Filter)dataRow["LastVisitedFilter"];
			bandStackFilter.LastVisited.ZoomFactor = (double)dataRow["LastVisitedZoomFactor"];
			bandStackFilter.LastVisited.ZoomSlider = (int)dataRow["LastVisitedZoomSlider"];
			string filterExpression = "FilterGUID = '" + bandStackFilter.GUID + "'";
			DataRow[] array2 = ds.Tables["BandStack2FilterFrequencies"].Select(filterExpression);
			foreach (DataRow dataRow2 in array2)
			{
				BandFrequencyData item = new BandFrequencyData
				{
					low = (double)dataRow2["Low"],
					high = (double)dataRow2["High"],
					lowOnly = (bool)dataRow2["LowOnly"],
					band = (Band)dataRow2["Band"],
					bandType = (BandType)dataRow2["BandType"],
					region = (FRSRegion)dataRow2["Region"]
				};
				bandStackFilter.FrequenciesToFilterOn.Add(item);
			}
			array2 = ds.Tables["BandStack2FilterModes"].Select(filterExpression);
			foreach (DataRow dataRow3 in array2)
			{
				bandStackFilter.ModesToFilterOn.Add((DSPMode)dataRow3["Mode"]);
			}
			array2 = ds.Tables["BandStack2FilterSubModes"].Select(filterExpression);
			foreach (DataRow dataRow4 in array2)
			{
				bandStackFilter.SubModesToFilterOn.Add((DSPSubMode)dataRow4["SubMode"]);
			}
			array2 = ds.Tables["BandStack2FilterBands"].Select(filterExpression);
			foreach (DataRow dataRow5 in array2)
			{
				bandStackFilter.BandsToFilterOn.Add((Band)dataRow5["Band"]);
			}
			bandStackFilter.IndexOfCurrentBlind = (int)dataRow["CurrentSelectedIndex"];
			bandStackFilter.GuidOfCurrentBlind = (string)dataRow["CurrentSelectedGUID"];
			if (!dictionary.ContainsKey(bandStackFilter.FilterName))
			{
				dictionary.Add(bandStackFilter.FilterName, bandStackFilter);
			}
		}
		return dictionary;
	}

	private static void checkForPrimaryKeys(string name)
	{
		DataColumn[] array = new DataColumn[1] { ds.Tables[name].Columns["Key"] };
		if (ds.Tables[name].PrimaryKey != array)
		{
			ds.Tables[name].PrimaryKey = array;
		}
	}

	private static void AddFormTable(string name)
	{
		ds.Tables.Add(name);
		ds.Tables[name].Columns.Add("Key", typeof(string));
		ds.Tables[name].Columns.Add("Value", typeof(string));
		checkForPrimaryKeys(name);
	}

	private static void AddBandTextTable()
	{
		ds.Tables.Add("BandText");
		DataTable dataTable = ds.Tables["BandText"];
		dataTable.Columns.Add("Low", typeof(double));
		dataTable.Columns.Add("High", typeof(double));
		dataTable.Columns.Add("Name", typeof(string));
		dataTable.Columns.Add("TX", typeof(bool));
		object[] array = new object[1108]
		{
			1.8, 1.809999, "160M CW/Digital Modes", true, 1.81, 1.81, "160M CW QRP", true, 1.810001, 1.842999,
			"160M CW", true, 1.843, 1.909999, "160M SSB/SSTV/Wide Band", true, 1.91, 1.91, "160M SSB QRP", true,
			1.910001, 1.994999, "160M SSB/SSTV/Wide Band", true, 1.995, 1.999999, "160M Experimental", true, 3.5, 3.524999,
			"80M Extra CW", true, 3.525, 3.579999, "80M CW", true, 3.58, 3.589999, "80M RTTY", true,
			3.59, 3.59, "80M RTTY DX", true, 3.590001, 3.599999, "80M RTTY", true, 3.6, 3.699999,
			"75M Extra SSB", true, 3.7, 3.789999, "75M Ext/Adv SSB", true, 3.79, 3.799999, "75M Ext/Adv DX Window", true,
			3.8, 3.844999, "75M SSB", true, 3.845, 3.845, "75M SSTV", true, 3.845001, 3.884999,
			"75M SSB", true, 3.885, 3.885, "75M AM Calling Frequency", true, 3.885001, 3.999999, "75M SSB", true,
			5.1, 5.331999, "60M General", false, 5.332, 5.332, "60M Channel 1", true, 5.332001, 5.347999,
			"60M General", false, 5.348, 5.348, "60M Channel 2", true, 5.348001, 5.358499, "60M General", false,
			5.3585, 5.3585, "60M Channel 3", true, 5.358501, 5.372999, "60M General", false, 5.373, 5.373,
			"60M Channel 4", true, 5.373001, 5.404999, "60M General", false, 5.405, 5.405, "60M Channel 5", true,
			5.405001, 5.499999, "60M General", false, 7.0, 7.024999, "40M Extra CW", true, 7.025, 7.039999,
			"40M CW", true, 7.04, 7.04, "40M RTTY DX", true, 7.040001, 7.099999, "40M RTTY", true,
			7.1, 7.124999, "40M CW", true, 7.125, 7.170999, "40M Ext/Adv SSB", true, 7.171, 7.171,
			"40M SSTV", true, 7.171001, 7.174999, "40M Ext/Adv SSB", true, 7.175, 7.289999, "40M SSB", true,
			7.29, 7.29, "40M AM Calling Frequency", true, 7.290001, 7.299999, "40M SSB", true, 10.1, 10.129999,
			"30M CW", true, 10.13, 10.139999, "30M RTTY", true, 10.14, 10.149999, "30M Packet", true,
			14.0, 14.024999, "20M Extra CW", true, 14.025, 14.069999, "20M CW", true, 14.07, 14.094999,
			"20M RTTY", true, 14.095, 14.099499, "20M Packet", true, 14.0995, 14.099999, "20M CW", true,
			14.1, 14.1, "20M NCDXF Beacons", true, 14.100001, 14.100499, "20M CW", true, 14.1005, 14.111999,
			"20M Packet", true, 14.112, 14.149999, "20M CW", true, 14.15, 14.174999, "20M Extra SSB", true,
			14.175, 14.224999, "20M Ext/Adv SSB", true, 14.225, 14.229999, "20M SSB", true, 14.23, 14.23,
			"20M SSTV", true, 14.230001, 14.285999, "20M SSB", true, 14.286, 14.286, "20M AM Calling Frequency", true,
			14.286001, 14.349999, "20M SSB", true, 18.068, 18.099999, "17M CW", true, 18.1, 18.104999,
			"17M RTTY", true, 18.105, 18.109999, "17M Packet", true, 18.11, 18.11, "17M NCDXF Beacons", true,
			18.110001, 18.167999, "17M SSB", true, 21.0, 21.024999, "15M Extra CW", true, 21.025, 21.069999,
			"15M CW", true, 21.07, 21.099999, "15M RTTY", true, 21.1, 21.109999, "15M Packet", true,
			21.11, 21.149999, "15M CW", true, 21.15, 21.15, "15M NCDXF Beacons", true, 21.150001, 21.199999,
			"15M CW", true, 21.2, 21.224999, "15M Extra SSB", true, 21.225, 21.274999, "15M Ext/Adv SSB", true,
			21.275, 21.339999, "15M SSB", true, 21.34, 21.34, "15M SSTV", true, 21.340001, 21.449999,
			"15M SSB", true, 24.89, 24.919999, "12M CW", true, 24.92, 24.924999, "12M RTTY", true,
			24.925, 24.929999, "12M Packet", true, 24.93, 24.93, "12M NCDXF Beacons", true, 24.930001, 24.989999,
			"12M SSB", true, 28.0, 28.069999, "10M CW", true, 28.07, 28.149999, "10M RTTY", true,
			28.15, 28.199999, "10M CW", true, 28.2, 28.2, "10M NCDXF Beacons", true, 28.200001, 28.299999,
			"10M Beacons", true, 28.3, 28.679999, "10M SSB", true, 28.68, 28.68, "10M SSTV", true,
			28.680001, 28.999999, "10M SSB", true, 29.0, 29.199999, "10M AM", true, 29.2, 29.299999,
			"10M SSB", true, 29.3, 29.509999, "10M Satellite Downlinks", true, 29.51, 29.519999, "10M Deadband", true,
			29.52, 29.589999, "10M Repeater Inputs", true, 29.59, 29.599999, "10M Deadband", true, 29.6, 29.6,
			"10M FM Simplex", true, 29.600001, 29.609999, "10M Deadband", true, 29.61, 29.699999, "10M Repeater Outputs", true,
			50.0, 50.059999, "6M CW", true, 50.06, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.099999,
			"6M CW", true, 50.1, 50.124999, "6M DX Window", true, 50.125, 50.125, "6M Calling Frequency", true,
			50.125001, 50.299999, "6M SSB", true, 50.3, 50.599999, "6M All Modes", true, 50.6, 50.619999,
			"6M Non Voice", true, 50.62, 50.62, "6M Digital Packet Calling", true, 50.620001, 50.799999, "6M Non Voice", true,
			50.8, 50.999999, "6M RC", true, 51.0, 51.099999, "6M Pacific DX Window", true, 51.1, 51.119999,
			"6M Deadband", true, 51.12, 51.179999, "6M Digital Repeater Inputs", true, 51.18, 51.479999, "6M Repeater Inputs", true,
			51.48, 51.619999, "6M Deadband", true, 51.62, 51.679999, "6M Digital Repeater Outputs", true, 51.68, 51.979999,
			"6M Repeater Outputs", true, 51.98, 51.999999, "6M Deadband", true, 52.0, 52.019999, "6M Repeater Inputs", true,
			52.02, 52.02, "6M FM Simplex", true, 52.020001, 52.039999, "6M Repeater Inputs", true, 52.04, 52.04,
			"6M FM Simplex", true, 52.040001, 52.479999, "6M Repeater Inputs", true, 52.48, 52.499999, "6M Deadband", true,
			52.5, 52.524999, "6M Repeater Outputs", true, 52.525, 52.525, "6M Primary FM Simplex", true, 52.525001, 52.539999,
			"6M Deadband", true, 52.54, 52.54, "6M Secondary FM Simplex", true, 52.540001, 52.979999, "6M Repeater Outputs", true,
			52.98, 52.999999, "6M Deadbands", true, 53.0, 53.0, "6M Remote Base FM Spx", true, 53.000001, 53.019999,
			"6M Repeater Inputs", true, 53.02, 53.02, "6M FM Simplex", true, 53.020001, 53.479999, "6M Repeater Inputs", true,
			53.48, 53.499999, "6M Deadband", true, 53.5, 53.519999, "6M Repeater Outputs", true, 53.52, 53.52,
			"6M FM Simplex", true, 53.520001, 53.899999, "6M Repeater Outputs", true, 53.9, 53.9, "6M FM Simplex", true,
			53.90001, 53.979999, "6M Repeater Outputs", true, 53.98, 53.999999, "6M Deadband", true, 144.0, 144.099999,
			"2M CW", true, 144.1, 144.199999, "2M CW/SSB", true, 144.2, 144.2, "2M Calling", true,
			144.200001, 144.274999, "2M CW/SSB", true, 144.275, 144.299999, "2M Beacon Sub-Band", true, 144.3, 144.499999,
			"2M Satellite", true, 144.5, 144.599999, "2M Linear Translator Inputs", true, 144.6, 144.899999, "2M FM Repeater", true,
			144.9, 145.199999, "2M FM Simplex", true, 145.2, 145.499999, "2M FM Repeater", true, 145.5, 145.799999,
			"2M FM Simplex", true, 145.8, 145.999999, "2M Satellite", true, 146.0, 146.399999, "2M FM Repeater", true,
			146.4, 146.609999, "2M FM Simplex", true, 146.61, 147.389999, "2M FM Repeater", true, 147.39, 147.599999,
			"2M FM Simplex", true, 147.6, 147.999999, "2M FM Repeater", true, 222.0, 222.024999, "1.25M EME/Weak Signal", true,
			222.025, 222.049999, "1.25M Weak Signal", true, 222.05, 222.059999, "1.25M Propagation Beacons", true, 222.06, 222.099999,
			"1.25M Weak Signal", true, 222.1, 222.1, "1.25M SSB/CW Calling", true, 222.100001, 222.149999, "1.25M Weak Signal CW/SSB", true,
			222.15, 222.249999, "1.25M Local Option", true, 222.25, 223.38, "1.25M FM Repeater Inputs", true, 223.380001, 223.399999,
			"1.25M General", true, 223.4, 223.519999, "1.25M FM Simplex", true, 223.52, 223.639999, "1.25M Digital/Packet", true,
			223.64, 223.7, "1.25M Links/Control", true, 223.700001, 223.709999, "1.25M General", true, 223.71, 223.849999,
			"1.25M Local Option", true, 223.85, 224.98, "1.25M Repeater Outputs", true, 420.0, 425.999999, "70cm ATV Repeater", true,
			426.0, 431.999999, "70cm ATV Simplex", true, 432.0, 432.069999, "70cm EME", true, 432.07, 432.099999,
			"70cm Weak Signal CW", true, 432.1, 432.1, "70cm Calling Frequency", true, 432.100001, 432.299999, "70cm Mixed Mode Weak Signal", true,
			432.3, 432.399999, "70cm Propagation Beacons", true, 432.4, 432.999999, "70cm Mixed Mode Weak Signal", true, 433.0, 434.999999,
			"70cm Auxillary/Repeater Links", true, 435.0, 437.999999, "70cm Satellite Only", true, 438.0, 441.999999, "70cm ATV Repeater", true,
			442.0, 444.999999, "70cm Local Repeaters", true, 445.0, 445.999999, "70cm Local Option", true, 446.0, 446.0,
			"70cm Simplex", true, 446.000001, 446.999999, "70cm Local Option", true, 447.0, 450.0, "70cm Local Repeaters", true,
			902.0, 902.099999, "33cm Weak Signal SSTV/FAX/ACSSB", true, 902.1, 902.1, "33cm Weak Signal Calling", true, 902.100001, 902.799999,
			"33cm Weak Signal SSTV/FAX/ACSSB", true, 902.8, 902.999999, "33cm Weak Signal EME/CW", true, 903.0, 903.099999, "33cm Digital Modes", true,
			903.1, 903.1, "33cm Alternate Calling", true, 903.100001, 905.999999, "33cm Digital Modes", true, 906.0, 908.999999,
			"33cm FM Repeater Inputs", true, 909.0, 914.999999, "33cm ATV", true, 915.0, 917.999999, "33cm Digital Modes", true,
			918.0, 920.999999, "33cm FM Repeater Outputs", true, 921.0, 926.999999, "33cm ATV", true, 927.0, 928.0,
			"33cm FM Simplex/Links", true, 1240.0, 1245.999999, "23cm ATV #1", true, 1246.0, 1251.999999, "23cm FM Point/Links", true,
			1252.0, 1257.999999, "23cm ATV #2, Digital Modes", true, 1258.0, 1259.999999, "23cm FM Point/Links", true, 1260.0, 1269.999999,
			"23cm Sat Uplinks/Wideband Exp.", true, 1270.0, 1275.999999, "23cm Repeater Inputs", true, 1276.0, 1281.999999, "23cm ATV #3", true,
			1282.0, 1287.999999, "23cm Repeater Outputs", true, 1288.0, 1293.999999, "23cm Simplex ATV/Wideband Exp.", true, 1294.0, 1294.499999,
			"23cm Simplex FM", true, 1294.5, 1294.5, "23cm FM Simplex Calling", true, 1294.500001, 1294.999999, "23cm Simplex FM", true,
			1295.0, 1295.799999, "23cm SSTV/FAX/ACSSB/Exp.", true, 1295.8, 1295.999999, "23cm EME/CW Expansion", true, 1296.0, 1296.049999,
			"23cm EME Exclusive", true, 1296.05, 1296.069999, "23cm Weak Signal", true, 1296.07, 1296.079999, "23cm CW Beacons", true,
			1296.08, 1296.099999, "23cm Weak Signal", true, 1296.1, 1296.1, "23cm CW/SSB Calling", true, 1296.100001, 1296.399999,
			"23cm Weak Signal", true, 1296.4, 1296.599999, "23cm X-Band Translator Input", true, 1296.6, 1296.799999, "23cm X-Band Translator Output", true,
			1296.8, 1296.999999, "23cm Experimental Beacons", true, 1297.0, 1300.0, "23cm Digital Modes", true, 2300.0, 2302.999999,
			"2.3GHz High Data Rate", true, 2303.0, 2303.499999, "2.3GHz Packet", true, 2303.5, 2303.8, "2.3GHz TTY Packet", true,
			2303.800001, 2303.899999, "2.3GHz General", true, 2303.9, 2303.9, "2.3GHz Packet/TTY/CW/EME", true, 2303.900001, 2304.099999,
			"2.3GHz CW/EME", true, 2304.1, 2304.1, "2.3GHz Calling Frequency", true, 2304.100001, 2304.199999, "2.3GHz CW/EME/SSB", true,
			2304.2, 2304.299999, "2.3GHz SSB/SSTV/FAX/Packet AM/Amtor", true, 2304.3, 2304.319999, "2.3GHz Propagation Beacon Network", true, 2304.32, 2304.399999,
			"2.3GHz General Propagation Beacons", true, 2304.4, 2304.499999, "2.3GHz SSB/SSTV/ACSSB/FAX/Packet AM", true, 2304.5, 2304.699999, "2.3GHz X-Band Translator Input", true,
			2304.7, 2304.899999, "2.3GHz X-Band Translator Output", true, 2304.9, 2304.999999, "2.3GHz Experimental Beacons", true, 2305.0, 2305.199999,
			"2.3GHz FM Simplex", true, 2305.2, 2305.2, "2.3GHz FM Simplex Calling", true, 2305.200001, 2305.999999, "2.3GHz FM Simplex", true,
			2306.0, 2308.999999, "2.3GHz FM Repeaters", true, 2309.0, 2310.0, "2.3GHz Control/Aux Links", true, 2390.0, 2395.999999,
			"2.3GHz Fast-Scan TV", true, 2396.0, 2398.999999, "2.3GHz High Rate Data", true, 2399.0, 2399.499999, "2.3GHz Packet", true,
			2399.5, 2399.999999, "2.3GHz Control/Aux Links", true, 2400.0, 2402.999999, "2.4GHz Satellite", true, 2403.0, 2407.999999,
			"2.4GHz Satellite High-Rate Data", true, 2408.0, 2409.999999, "2.4GHz Satellite", true, 2410.0, 2412.999999, "2.4GHz FM Repeaters", true,
			2413.0, 2417.999999, "2.4GHz High-Rate Data", true, 2418.0, 2429.999999, "2.4GHz Fast-Scan TV", true, 2430.0, 2432.999999,
			"2.4GHz Satellite", true, 2433.0, 2437.999999, "2.4GHz Sat. High-Rate Data", true, 2438.0, 2450.0, "2.4GHz Wideband FM/FSTV/FMTV", true,
			3456.0, 3456.099999, "3.4GHz General", true, 3456.1, 3456.1, "3.4GHz Calling Frequency", true, 3456.100001, 3456.299999,
			"3.4GHz General", true, 3456.3, 3456.4, "3.4GHz Propagation Beacons", true, 5760.0, 5760.099999, "5.7GHz General", true,
			5760.1, 5760.1, "5.7GHz Calling Frequency", true, 5760.100001, 5760.299999, "5.7GHz General", true, 5760.3, 5760.4,
			"5.7GHz Propagation Beacons", true, 10368.0, 10368.099999, "10GHz General", true, 10368.1, 10368.1, "10GHz Calling Frequency", true,
			10368.100001, 10368.4, "10GHz General", true, 24192.0, 24192.099999, "24GHz General", true, 24192.1, 24192.1,
			"24GHz Calling Frequency", true, 24192.100001, 24192.4, "24GHz General", true, 47088.0, 47088.099999, "47GHz General", true,
			47088.1, 47088.1, "47GHz Calling Frequency", true, 47088.100001, 47088.4, "47GHz General", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
		AddBandTextSWB();
	}

	private static void AddRegion2BandText()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[1108]
		{
			1.8, 1.809999, "160M CW/Digital Modes", true, 1.81, 1.81, "160M CW QRP", true, 1.810001, 1.842999,
			"160M CW", true, 1.843, 1.909999, "160M SSB/SSTV/Wide Band", true, 1.91, 1.91, "160M SSB QRP", true,
			1.910001, 1.994999, "160M SSB/SSTV/Wide Band", true, 1.995, 1.999999, "160M Experimental", true, 3.5, 3.524999,
			"80M Extra CW", true, 3.525, 3.579999, "80M CW", true, 3.58, 3.589999, "80M RTTY", true,
			3.59, 3.59, "80M RTTY DX", true, 3.590001, 3.599999, "80M RTTY", true, 3.6, 3.699999,
			"75M Extra SSB", true, 3.7, 3.789999, "75M Ext/Adv SSB", true, 3.79, 3.799999, "75M Ext/Adv DX Window", true,
			3.8, 3.844999, "75M SSB", true, 3.845, 3.845, "75M SSTV", true, 3.845001, 3.884999,
			"75M SSB", true, 3.885, 3.885, "75M AM Calling Frequency", true, 3.885001, 3.999999, "75M SSB", true,
			5.1, 5.331999, "60M General", false, 5.332, 5.332, "60M Channel 1", true, 5.332001, 5.347999,
			"60M General", false, 5.348, 5.348, "60M Channel 2", true, 5.348001, 5.358499, "60M General", false,
			5.3585, 5.3585, "60M Channel 3", true, 5.358501, 5.372999, "60M General", false, 5.373, 5.373,
			"60M Channel 4", true, 5.373001, 5.404999, "60M General", false, 5.405, 5.405, "60M Channel 5", true,
			5.405001, 5.499999, "60M General", false, 7.0, 7.024999, "40M Extra CW", true, 7.025, 7.039999,
			"40M CW", true, 7.04, 7.04, "40M RTTY DX", true, 7.040001, 7.099999, "40M RTTY", true,
			7.1, 7.124999, "40M CW", true, 7.125, 7.170999, "40M Ext/Adv SSB", true, 7.171, 7.171,
			"40M SSTV", true, 7.171001, 7.174999, "40M Ext/Adv SSB", true, 7.175, 7.289999, "40M SSB", true,
			7.29, 7.29, "40M AM Calling Frequency", true, 7.290001, 7.299999, "40M SSB", true, 10.1, 10.129999,
			"30M CW", true, 10.13, 10.139999, "30M RTTY", true, 10.14, 10.149999, "30M Packet", true,
			14.0, 14.024999, "20M Extra CW", true, 14.025, 14.069999, "20M CW", true, 14.07, 14.094999,
			"20M RTTY", true, 14.095, 14.099499, "20M Packet", true, 14.0995, 14.099999, "20M CW", true,
			14.1, 14.1, "20M NCDXF Beacons", true, 14.100001, 14.100499, "20M CW", true, 14.1005, 14.111999,
			"20M Packet", true, 14.112, 14.149999, "20M CW", true, 14.15, 14.174999, "20M Extra SSB", true,
			14.175, 14.224999, "20M Ext/Adv SSB", true, 14.225, 14.229999, "20M SSB", true, 14.23, 14.23,
			"20M SSTV", true, 14.230001, 14.285999, "20M SSB", true, 14.286, 14.286, "20M AM Calling Frequency", true,
			14.286001, 14.349999, "20M SSB", true, 18.068, 18.099999, "17M CW", true, 18.1, 18.104999,
			"17M RTTY", true, 18.105, 18.109999, "17M Packet", true, 18.11, 18.11, "17M NCDXF Beacons", true,
			18.110001, 18.167999, "17M SSB", true, 21.0, 21.024999, "15M Extra CW", true, 21.025, 21.069999,
			"15M CW", true, 21.07, 21.099999, "15M RTTY", true, 21.1, 21.109999, "15M Packet", true,
			21.11, 21.149999, "15M CW", true, 21.15, 21.15, "15M NCDXF Beacons", true, 21.150001, 21.199999,
			"15M CW", true, 21.2, 21.224999, "15M Extra SSB", true, 21.225, 21.274999, "15M Ext/Adv SSB", true,
			21.275, 21.339999, "15M SSB", true, 21.34, 21.34, "15M SSTV", true, 21.340001, 21.449999,
			"15M SSB", true, 24.89, 24.919999, "12M CW", true, 24.92, 24.924999, "12M RTTY", true,
			24.925, 24.929999, "12M Packet", true, 24.93, 24.93, "12M NCDXF Beacons", true, 24.930001, 24.989999,
			"12M SSB", true, 28.0, 28.069999, "10M CW", true, 28.07, 28.149999, "10M RTTY", true,
			28.15, 28.199999, "10M CW", true, 28.2, 28.2, "10M NCDXF Beacons", true, 28.200001, 28.299999,
			"10M Beacons", true, 28.3, 28.679999, "10M SSB", true, 28.68, 28.68, "10M SSTV", true,
			28.680001, 28.999999, "10M SSB", true, 29.0, 29.199999, "10M AM", true, 29.2, 29.299999,
			"10M SSB", true, 29.3, 29.509999, "10M Satellite Downlinks", true, 29.51, 29.519999, "10M Deadband", true,
			29.52, 29.589999, "10M Repeater Inputs", true, 29.59, 29.599999, "10M Deadband", true, 29.6, 29.6,
			"10M FM Simplex", true, 29.600001, 29.609999, "10M Deadband", true, 29.61, 29.699999, "10M Repeater Outputs", true,
			50.0, 50.059999, "6M CW", true, 50.06, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.099999,
			"6M CW", true, 50.1, 50.124999, "6M DX Window", true, 50.125, 50.125, "6M Calling Frequency", true,
			50.125001, 50.299999, "6M SSB", true, 50.3, 50.599999, "6M All Modes", true, 50.6, 50.619999,
			"6M Non Voice", true, 50.62, 50.62, "6M Digital Packet Calling", true, 50.620001, 50.799999, "6M Non Voice", true,
			50.8, 50.999999, "6M RC", true, 51.0, 51.099999, "6M Pacific DX Window", true, 51.1, 51.119999,
			"6M Deadband", true, 51.12, 51.179999, "6M Digital Repeater Inputs", true, 51.18, 51.479999, "6M Repeater Inputs", true,
			51.48, 51.619999, "6M Deadband", true, 51.62, 51.679999, "6M Digital Repeater Outputs", true, 51.68, 51.979999,
			"6M Repeater Outputs", true, 51.98, 51.999999, "6M Deadband", true, 52.0, 52.019999, "6M Repeater Inputs", true,
			52.02, 52.02, "6M FM Simplex", true, 52.020001, 52.039999, "6M Repeater Inputs", true, 52.04, 52.04,
			"6M FM Simplex", true, 52.040001, 52.479999, "6M Repeater Inputs", true, 52.48, 52.499999, "6M Deadband", true,
			52.5, 52.524999, "6M Repeater Outputs", true, 52.525, 52.525, "6M Primary FM Simplex", true, 52.525001, 52.539999,
			"6M Deadband", true, 52.54, 52.54, "6M Secondary FM Simplex", true, 52.540001, 52.979999, "6M Repeater Outputs", true,
			52.98, 52.999999, "6M Deadbands", true, 53.0, 53.0, "6M Remote Base FM Spx", true, 53.000001, 53.019999,
			"6M Repeater Inputs", true, 53.02, 53.02, "6M FM Simplex", true, 53.020001, 53.479999, "6M Repeater Inputs", true,
			53.48, 53.499999, "6M Deadband", true, 53.5, 53.519999, "6M Repeater Outputs", true, 53.52, 53.52,
			"6M FM Simplex", true, 53.520001, 53.899999, "6M Repeater Outputs", true, 53.9, 53.9, "6M FM Simplex", true,
			53.90001, 53.979999, "6M Repeater Outputs", true, 53.98, 53.999999, "6M Deadband", true, 144.0, 144.099999,
			"2M CW", true, 144.1, 144.199999, "2M CW/SSB", true, 144.2, 144.2, "2M Calling", true,
			144.200001, 144.274999, "2M CW/SSB", true, 144.275, 144.299999, "2M Beacon Sub-Band", true, 144.3, 144.499999,
			"2M Satellite", true, 144.5, 144.599999, "2M Linear Translator Inputs", true, 144.6, 144.899999, "2M FM Repeater", true,
			144.9, 145.199999, "2M FM Simplex", true, 145.2, 145.499999, "2M FM Repeater", true, 145.5, 145.799999,
			"2M FM Simplex", true, 145.8, 145.999999, "2M Satellite", true, 146.0, 146.399999, "2M FM Repeater", true,
			146.4, 146.609999, "2M FM Simplex", true, 146.61, 147.389999, "2M FM Repeater", true, 147.39, 147.599999,
			"2M FM Simplex", true, 147.6, 147.999999, "2M FM Repeater", true, 222.0, 222.024999, "1.25M EME/Weak Signal", true,
			222.025, 222.049999, "1.25M Weak Signal", true, 222.05, 222.059999, "1.25M Propagation Beacons", true, 222.06, 222.099999,
			"1.25M Weak Signal", true, 222.1, 222.1, "1.25M SSB/CW Calling", true, 222.100001, 222.149999, "1.25M Weak Signal CW/SSB", true,
			222.15, 222.249999, "1.25M Local Option", true, 222.25, 223.38, "1.25M FM Repeater Inputs", true, 223.380001, 223.399999,
			"1.25M General", true, 223.4, 223.519999, "1.25M FM Simplex", true, 223.52, 223.639999, "1.25M Digital/Packet", true,
			223.64, 223.7, "1.25M Links/Control", true, 223.700001, 223.709999, "1.25M General", true, 223.71, 223.849999,
			"1.25M Local Option", true, 223.85, 224.98, "1.25M Repeater Outputs", true, 420.0, 425.999999, "70cm ATV Repeater", true,
			426.0, 431.999999, "70cm ATV Simplex", true, 432.0, 432.069999, "70cm EME", true, 432.07, 432.099999,
			"70cm Weak Signal CW", true, 432.1, 432.1, "70cm Calling Frequency", true, 432.100001, 432.299999, "70cm Mixed Mode Weak Signal", true,
			432.3, 432.399999, "70cm Propagation Beacons", true, 432.4, 432.999999, "70cm Mixed Mode Weak Signal", true, 433.0, 434.999999,
			"70cm Auxillary/Repeater Links", true, 435.0, 437.999999, "70cm Satellite Only", true, 438.0, 441.999999, "70cm ATV Repeater", true,
			442.0, 444.999999, "70cm Local Repeaters", true, 445.0, 445.999999, "70cm Local Option", true, 446.0, 446.0,
			"70cm Simplex", true, 446.000001, 446.999999, "70cm Local Option", true, 447.0, 450.0, "70cm Local Repeaters", true,
			902.0, 902.099999, "33cm Weak Signal SSTV/FAX/ACSSB", true, 902.1, 902.1, "33cm Weak Signal Calling", true, 902.100001, 902.799999,
			"33cm Weak Signal SSTV/FAX/ACSSB", true, 902.8, 902.999999, "33cm Weak Signal EME/CW", true, 903.0, 903.099999, "33cm Digital Modes", true,
			903.1, 903.1, "33cm Alternate Calling", true, 903.100001, 905.999999, "33cm Digital Modes", true, 906.0, 908.999999,
			"33cm FM Repeater Inputs", true, 909.0, 914.999999, "33cm ATV", true, 915.0, 917.999999, "33cm Digital Modes", true,
			918.0, 920.999999, "33cm FM Repeater Outputs", true, 921.0, 926.999999, "33cm ATV", true, 927.0, 928.0,
			"33cm FM Simplex/Links", true, 1240.0, 1245.999999, "23cm ATV #1", true, 1246.0, 1251.999999, "23cm FM Point/Links", true,
			1252.0, 1257.999999, "23cm ATV #2, Digital Modes", true, 1258.0, 1259.999999, "23cm FM Point/Links", true, 1260.0, 1269.999999,
			"23cm Sat Uplinks/Wideband Exp.", true, 1270.0, 1275.999999, "23cm Repeater Inputs", true, 1276.0, 1281.999999, "23cm ATV #3", true,
			1282.0, 1287.999999, "23cm Repeater Outputs", true, 1288.0, 1293.999999, "23cm Simplex ATV/Wideband Exp.", true, 1294.0, 1294.499999,
			"23cm Simplex FM", true, 1294.5, 1294.5, "23cm FM Simplex Calling", true, 1294.500001, 1294.999999, "23cm Simplex FM", true,
			1295.0, 1295.799999, "23cm SSTV/FAX/ACSSB/Exp.", true, 1295.8, 1295.999999, "23cm EME/CW Expansion", true, 1296.0, 1296.049999,
			"23cm EME Exclusive", true, 1296.05, 1296.069999, "23cm Weak Signal", true, 1296.07, 1296.079999, "23cm CW Beacons", true,
			1296.08, 1296.099999, "23cm Weak Signal", true, 1296.1, 1296.1, "23cm CW/SSB Calling", true, 1296.100001, 1296.399999,
			"23cm Weak Signal", true, 1296.4, 1296.599999, "23cm X-Band Translator Input", true, 1296.6, 1296.799999, "23cm X-Band Translator Output", true,
			1296.8, 1296.999999, "23cm Experimental Beacons", true, 1297.0, 1300.0, "23cm Digital Modes", true, 2300.0, 2302.999999,
			"2.3GHz High Data Rate", true, 2303.0, 2303.499999, "2.3GHz Packet", true, 2303.5, 2303.8, "2.3GHz TTY Packet", true,
			2303.800001, 2303.899999, "2.3GHz General", true, 2303.9, 2303.9, "2.3GHz Packet/TTY/CW/EME", true, 2303.900001, 2304.099999,
			"2.3GHz CW/EME", true, 2304.1, 2304.1, "2.3GHz Calling Frequency", true, 2304.100001, 2304.199999, "2.3GHz CW/EME/SSB", true,
			2304.2, 2304.299999, "2.3GHz SSB/SSTV/FAX/Packet AM/Amtor", true, 2304.3, 2304.319999, "2.3GHz Propagation Beacon Network", true, 2304.32, 2304.399999,
			"2.3GHz General Propagation Beacons", true, 2304.4, 2304.499999, "2.3GHz SSB/SSTV/ACSSB/FAX/Packet AM", true, 2304.5, 2304.699999, "2.3GHz X-Band Translator Input", true,
			2304.7, 2304.899999, "2.3GHz X-Band Translator Output", true, 2304.9, 2304.999999, "2.3GHz Experimental Beacons", true, 2305.0, 2305.199999,
			"2.3GHz FM Simplex", true, 2305.2, 2305.2, "2.3GHz FM Simplex Calling", true, 2305.200001, 2305.999999, "2.3GHz FM Simplex", true,
			2306.0, 2308.999999, "2.3GHz FM Repeaters", true, 2309.0, 2310.0, "2.3GHz Control/Aux Links", true, 2390.0, 2395.999999,
			"2.3GHz Fast-Scan TV", true, 2396.0, 2398.999999, "2.3GHz High Rate Data", true, 2399.0, 2399.499999, "2.3GHz Packet", true,
			2399.5, 2399.999999, "2.3GHz Control/Aux Links", true, 2400.0, 2402.999999, "2.4GHz Satellite", true, 2403.0, 2407.999999,
			"2.4GHz Satellite High-Rate Data", true, 2408.0, 2409.999999, "2.4GHz Satellite", true, 2410.0, 2412.999999, "2.4GHz FM Repeaters", true,
			2413.0, 2417.999999, "2.4GHz High-Rate Data", true, 2418.0, 2429.999999, "2.4GHz Fast-Scan TV", true, 2430.0, 2432.999999,
			"2.4GHz Satellite", true, 2433.0, 2437.999999, "2.4GHz Sat. High-Rate Data", true, 2438.0, 2450.0, "2.4GHz Wideband FM/FSTV/FMTV", true,
			3456.0, 3456.099999, "3.4GHz General", true, 3456.1, 3456.1, "3.4GHz Calling Frequency", true, 3456.100001, 3456.299999,
			"3.4GHz General", true, 3456.3, 3456.4, "3.4GHz Propagation Beacons", true, 5760.0, 5760.099999, "5.7GHz General", true,
			5760.1, 5760.1, "5.7GHz Calling Frequency", true, 5760.100001, 5760.299999, "5.7GHz General", true, 5760.3, 5760.4,
			"5.7GHz Propagation Beacons", true, 10368.0, 10368.099999, "10GHz General", true, 10368.1, 10368.1, "10GHz Calling Frequency", true,
			10368.100001, 10368.4, "10GHz General", true, 24192.0, 24192.099999, "24GHz General", true, 24192.1, 24192.1,
			"24GHz Calling Frequency", true, 24192.100001, 24192.4, "24GHz General", true, 47088.0, 47088.099999, "47GHz General", true,
			47088.1, 47088.1, "47GHz Calling Frequency", true, 47088.100001, 47088.4, "47GHz General", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
		AddBandTextSWB();
	}

	private static void AddRegionJapanBandText()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[216]
		{
			0.1357, 0.137799, "2200M CW/DATA", true, 0.472, 0.478999, "630M CW/DATA", true, 1.8, 1.825,
			"160M CW", true, 1.9075, 1.912499, "160M CW/DATA", true, 3.5, 3.519999, "80M CW", true,
			3.52, 3.534999, "80M CW/DATA", true, 3.535, 3.574999, "80M CW/SSB/SSTV", true, 3.599, 3.611999,
			"75M CW/SSB/SSTV/DATA", true, 3.68, 3.686999, "75M CW/SSB/SSTV", true, 3.702, 3.715999, "75M CW/SSB/SSTV", true,
			3.745, 3.769999, "75M CW/SSB/SSTV", true, 3.791, 3.804999, "75M CW/SSB/SSTV", true, 7.0, 7.029999,
			"40M CW", true, 7.03, 7.044999, "40M CW/DATA", true, 7.045, 7.099999, "40M CW/SSB/SSTV", true,
			7.1, 7.199999, "40M All Modes", true, 10.1, 10.129999, "30M CW", true, 10.13, 10.149999,
			"30M CW/DATA", true, 14.0, 14.069999, "20M CW", true, 14.07, 14.099999, "20M CW/DATA", true,
			14.1, 14.1, "20M IBP Beacon", true, 14.100001, 14.111999, "20M CW/DATA", true, 14.112, 14.349999,
			"20M CW/SSB/SSTV", true, 18.068, 18.099999, "17M CW", true, 18.1, 18.109999, "17M CW/DATA", true,
			18.11, 18.11, "17M IBP Beacon", true, 18.110001, 18.167999, "17M CW/SSB/SSTV", true, 21.0, 21.069999,
			"15M CW", true, 21.07, 21.124999, "15M CW/DATA", true, 21.125, 21.149999, "15M CW/SSB/SSTV", true,
			21.15, 21.15, "15M IBP Beacon", true, 21.150001, 21.449999, "15M CW/SSB/SSTV", true, 24.89, 24.909999,
			"12M CW", true, 24.91, 24.929999, "12M CW/DATA", true, 24.93, 24.93, "12M IBP Beacon", true,
			24.930001, 24.989999, "12M CW/SSB/SSTV", true, 28.0, 28.069999, "10M CW", true, 28.07, 28.149999,
			"10M CW/DATA", true, 28.15, 28.199999, "10M CW", true, 28.2, 28.2, "10M IBP Beacon", true,
			28.200001, 28.999999, "10M CW/SSB/RTTY/SSTV", true, 29.0, 29.299999, "10M FM/RTTY/SSTV/DATA", true, 29.3, 29.509999,
			"10M Satellite Downlinks", true, 29.51, 29.589999, "10M Repeater Inputs", true, 29.59, 29.609999, "10M FM/RTTY/SSTV/DATA", true,
			29.61, 29.699999, "10M Repeater Outputs", true, 50.0, 50.099999, "6M CW", true, 50.1, 50.199999,
			"6M CW/SSB/RTTY/SSTV", true, 50.2, 50.999999, "6M CW/SSB/RTTY/SSTV/DATA", true, 51.0, 51.999999, "6M FM/RTTY/SSTV", true,
			52.0, 52.299999, "6M VoIP", true, 52.3, 52.499999, "6M CW/SSB/RTTY/SSTV", true, 52.5, 52.899999,
			"6M WB Data", true, 52.9, 53.999999, "6M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
		AddBandTextSWB();
	}

	private static void AddBandTextSWB()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[152]
		{
			2.5, 2.5, "WWV", false, 5.0, 5.0, "WWV", false, 10.0, 10.0,
			"WWV", false, 15.0, 15.0, "WWV", false, 20.0, 20.0, "WWV", false,
			25.0, 25.0, "WWV Time", false, 3.33, 3.33, "CHU", false, 7.85, 7.85,
			"CHU", false, 14.67, 14.67, "CHU", false, 4.996, 4.996, "RWM", false,
			9.996, 9.996, "RWM", false, 14.996, 14.996, "RWM", false, 4.998, 4.998,
			"EBC", false, 15.006, 15.006, "EBC", false, 0.1357, 0.137799, "2200M Band", true,
			0.153, 0.279, "AM - Long Wave", false, 0.415, 0.471999, "Maritime Band", false, 0.472, 0.478999,
			"630M Band", true, 0.479, 0.5264, "Maritime Band", false, 0.53, 1.71, "Broadcast AM Med Wave", false,
			2.3, 2.495, "120M Short Wave", false, 3.2, 3.329999, "90M Short Wave", false, 3.330001, 3.4,
			"90M Short Wave", false, 4.75, 4.995999, "60M Short Wave", false, 4.996001, 4.997999, "60M Short Wave", false,
			4.998001, 4.999999, "60M Short Wave", false, 5.000001, 5.06, "60M Short Wave", false, 5.9, 6.2,
			"49M Short Wave", false, 7.3, 7.35, "41M Short Wave", false, 9.4, 9.9, "31M Short Wave", false,
			11.6, 12.1, "25M Short Wave", false, 13.57, 13.87, "22M Short Wave", false, 15.1, 15.8,
			"19M Short Wave", false, 17.48, 17.9, "16M Short Wave", false, 18.9, 19.02, "15M Short Wave", false,
			21.45, 21.85, "13M Short Wave", false, 25.6, 26.1, "11M Short Wave", false, 26.33, 27.865,
			"11M Band", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void ClearBandText()
	{
		ds.Tables["BandText"].Clear();
	}

	private static void AddRegion1BandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[24]
		{
			1.81, 1.835999, "160M CW", true, 1.836, 1.836, "160M CW QRP", true, 1.836001, 1.837999,
			"160M CW", true, 1.838, 1.839999, "160M Narrow Band Modes", true, 1.84, 1.842999, "160M All Modes & Digital", true,
			1.843, 1.999999, "160M All Modes & Digital", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText80m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[36]
		{
			3.5, 3.559999, "80M CW", true, 3.56, 3.56, "80M CW QRP", true, 3.560001, 3.579999,
			"80M CW", true, 3.58, 3.599999, "80M Narrow Band Modes", true, 3.6, 3.689999, "80M All Modes", true,
			3.69, 3.69, "80M SSB QRP", true, 3.690001, 3.759999, "80M All Modes", true, 3.76, 3.76,
			"80M SSB Emergency", true, 3.760001, 3.799999, "80M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText60m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 5.1, 5.499999, "60M General RX", false };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText40m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[36]
		{
			7.0, 7.029999, "40M CW", true, 7.03, 7.03, "40M CW QRP", true, 7.030001, 7.034999,
			"40M CW", true, 7.035, 7.039999, "40M Narrow Band Modes", true, 7.04, 7.059999, "40M All Modes", true,
			7.06, 7.06, "40M SSB Emergency", true, 7.060001, 7.089999, "40M All Modes", true, 7.09, 7.09,
			"40M SSB QRP", true, 7.090001, 7.199999, "40M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText30m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[16]
		{
			10.1, 10.115999, "30M CW", true, 10.116, 10.116, "30M CW QRP", true, 10.116001, 10.139999,
			"30M CW", true, 10.14, 10.149999, "30M Narrow Band Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText20m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[60]
		{
			14.0, 14.059999, "20M CW", true, 14.06, 14.06, "20M CW QRP", true, 14.060001, 14.069999,
			"20M CW", true, 14.07, 14.098999, "20M Narrow Band Modes", true, 14.099, 14.100999, "20M Beacons", true,
			14.101, 14.111999, "20M All Mode Digital", true, 14.112, 14.129999, "20M All Mode Digital", true, 14.13, 14.13,
			"20M Digital Voice", true, 14.130001, 14.229999, "20M All Modes", true, 14.23, 14.23, "20M SSTV", true,
			14.230001, 14.284999, "20M All Modes", true, 14.285, 14.285, "20M SSB QRP", true, 14.285001, 14.299999,
			"20M All Modes", true, 14.3, 14.3, "20M SSB Emergency", true, 14.300001, 14.349999, "20M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText17m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[32]
		{
			18.068, 18.085999, "17M CW", true, 18.086, 18.086, "17M CW QRP", true, 18.086001, 18.094999,
			"17M CW", true, 18.095, 18.108999, "17M Narrow Band Modes", true, 18.109, 18.109999, "17M Beacons", true,
			18.11, 18.159999, "17M All Modes", true, 18.16, 18.16, "17M SSB Emergency", true, 18.160001, 18.167999,
			"17M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText15m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[56]
		{
			21.0, 21.059999, "15M CW", true, 21.06, 21.06, "15M CW QRP", true, 21.060001, 21.069999,
			"15M CW", true, 21.07, 21.109999, "15M Narrow Band Modes", true, 21.11, 21.119999, "15M Wide Band Digital", true,
			21.12, 21.148999, "15M Narrow Band Modes", true, 21.149, 21.150999, "15M Beacons", true, 21.151, 21.179999,
			"15M All Modes", true, 21.18, 21.18, "15M Digital Voice", true, 21.180001, 21.284999, "15M All Modes", true,
			21.285, 21.285, "15M SSB QRP", true, 21.285001, 21.359999, "15M All Modes", true, 21.36, 21.36,
			"15M SSB Emergency", true, 21.360001, 21.449999, "15M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText12m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[28]
		{
			24.89, 24.905999, "12M CW", true, 24.906, 24.906, "12M CW QRP", true, 24.906001, 24.914999,
			"12M CW", true, 24.915, 24.928999, "12M Narrow Band Modes", true, 24.929, 24.930999, "12M Beacons", true,
			24.931, 24.939999, "12M All Modes Digital", true, 24.94, 24.989999, "12M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText10m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[104]
		{
			28.0, 28.059999, "10M CW", true, 28.06, 28.06, "10M CW QRP", true, 28.060001, 28.069999,
			"10M CW", true, 28.07, 28.189999, "10M Narrow Band Modes", true, 28.19, 28.224999, "10M Beacons", true,
			28.225, 28.299999, "10M All Mode Beacons", true, 28.3, 28.319999, "10M All Mode Digital", true, 28.320001, 28.329999,
			"10M All Modes", true, 28.33, 28.33, "10M Digital Voice", true, 28.330001, 28.359999, "10M All Modes", true,
			28.36, 28.36, "10M SSB QRP", true, 28.360001, 28.679999, "10M All Modes", true, 28.68, 28.68,
			"10M SSTV", true, 28.680001, 29.199999, "10M All Modes", true, 29.2, 29.299999, "10M FM Digital", true,
			29.3, 29.509999, "10M FM Sat. Downlinks", true, 29.51, 29.519999, "10M Guard Channel", true, 29.52, 29.549999,
			"10M FM Simplex", true, 29.55, 29.559999, "10M Deadband", true, 29.56, 29.589999, "10M Repeater Inputs", true,
			29.59, 29.599999, "10M Deadband", true, 29.6, 29.6, "10M FM Calling", true, 29.600001, 29.609999,
			"10M Deadband", true, 29.61, 29.649999, "10M FM Simplex", true, 29.65, 29.659999, "10M Deadband", true,
			29.66, 29.699999, "10M Repeater Outputs", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[92]
		{
			50.0, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.089999, "6M CW", true, 50.09, 50.09,
			"6M CW Calling", true, 50.090001, 50.099999, "6M CW", true, 50.1, 50.109999, "6M CW & SSB", true,
			50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.129999, "6M CW, SSB & Digital", true, 50.13, 50.149999,
			"6M CW, SSB & Digital", true, 50.15, 50.15, "6M SSB Calling", true, 50.150001, 50.249999, "6M CW, SSB & Digital", true,
			50.25, 50.25, "6M PSK Calling", true, 50.250001, 50.499999, "6M CW, SSB & Digital", true, 50.5, 50.619999,
			"6M All Modes", true, 50.62, 50.749999, "6M Digital Comms.", true, 50.75, 51.209999, "6M All Modes", true,
			51.21, 51.389999, "6M FM Repeater Inputs", true, 51.39, 51.409999, "6M All Modes", true, 51.41, 51.509999,
			"6M FM Simplex", true, 51.51, 51.51, "6M FM Calling", true, 51.510001, 51.589999, "6M FM Simplex", true,
			51.59, 51.809999, "6M All Modes", true, 51.81, 51.989999, "6M FM Repeater Ouputs", true, 51.99, 51.999999,
			"6M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[24]
		{
			1.81, 1.835999, "160M CW", true, 1.836, 1.836, "160M CW QRP", true, 1.836001, 1.837999,
			"160M CW", true, 1.838, 1.839999, "160M Narrow Band Modes", true, 1.84, 1.842999, "160M All Modes & Digital", true,
			1.843, 1.86, "160M All Modes & Digital", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText80m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[32]
		{
			3.5, 3.559999, "80M CW", true, 3.56, 3.56, "80M CW QRP", true, 3.560001, 3.579999,
			"80M CW", true, 3.58, 3.599999, "80M Narrow Band Modes", true, 3.6, 3.689999, "80M All Modes", true,
			3.69, 3.69, "80M SSB QRP", true, 3.690001, 3.7, "80M All Modes", true, 3.89, 3.9,
			"80M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText40m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[36]
		{
			7.0, 7.029999, "40M CW", true, 7.03, 7.03, "40M CW QRP", true, 7.030001, 7.034999,
			"40M CW", true, 7.035, 7.039999, "40M Narrow Band Modes", true, 7.04, 7.059999, "40M All Modes", true,
			7.06, 7.06, "40M SSB Emergency", true, 7.060001, 7.089999, "40M All Modes", true, 7.09, 7.09,
			"40M SSB QRP", true, 7.090001, 7.199999, "40M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText20m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[60]
		{
			14.0, 14.059999, "20M CW", true, 14.06, 14.06, "20M CW QRP", true, 14.060001, 14.069999,
			"20M CW", true, 14.07, 14.098999, "20M Narrow Band Modes", true, 14.099, 14.100999, "20M Beacons", true,
			14.101, 14.111999, "20M All Mode Digital", true, 14.112, 14.129999, "20M All Mode Digital", true, 14.13, 14.13,
			"20M Digital Voice", true, 14.130001, 14.229999, "20M All Modes", true, 14.23, 14.23, "20M SSTV", true,
			14.230001, 14.284999, "20M All Modes", true, 14.285, 14.285, "20M SSB QRP", true, 14.285001, 14.299999,
			"20M All Modes", true, 14.3, 14.3, "20M SSB Emergency", true, 14.300001, 14.349999, "20M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText17m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[32]
		{
			18.068, 18.085999, "17M CW", true, 18.086, 18.086, "17M CW QRP", true, 18.086001, 18.094999,
			"17M CW", true, 18.095, 18.108999, "17M Narrow Band Modes", true, 18.109, 18.109999, "17M Beacons", true,
			18.11, 18.159999, "17M All Modes", true, 18.16, 18.16, "17M SSB Emergency", true, 18.160001, 18.167999,
			"17M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText15m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[56]
		{
			21.0, 21.059999, "15M CW", true, 21.06, 21.06, "15M CW QRP", true, 21.060001, 21.069999,
			"15M CW", true, 21.07, 21.109999, "15M Narrow Band Modes", true, 21.11, 21.119999, "15M Wide Band Digital", true,
			21.12, 21.148999, "15M Narrow Band Modes", true, 21.149, 21.150999, "15M Beacons", true, 21.151, 21.179999,
			"15M All Modes", true, 21.18, 21.18, "15M Digital Voice", true, 21.180001, 21.284999, "15M All Modes", true,
			21.285, 21.285, "15M SSB QRP", true, 21.285001, 21.359999, "15M All Modes", true, 21.36, 21.36,
			"15M SSB Emergency", true, 21.360001, 21.449999, "15M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText12m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[28]
		{
			24.89, 24.905999, "12M CW", true, 24.906, 24.906, "12M CW QRP", true, 24.906001, 24.914999,
			"12M CW", true, 24.915, 24.928999, "12M Narrow Band Modes", true, 24.929, 24.930999, "12M Beacons", true,
			24.931, 24.939999, "12M All Modes Digital", true, 24.94, 24.989999, "12M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText10m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[104]
		{
			28.0, 28.059999, "10M CW", true, 28.06, 28.06, "10M CW QRP", true, 28.060001, 28.069999,
			"10M CW", true, 28.07, 28.189999, "10M Narrow Band Modes", true, 28.19, 28.224999, "10M Beacons", true,
			28.225, 28.299999, "10M All Mode Beacons", true, 28.3, 28.319999, "10M All Mode Digital", true, 28.320001, 28.329999,
			"10M All Modes", true, 28.33, 28.33, "10M Digital Voice", true, 28.330001, 28.359999, "10M All Modes", true,
			28.36, 28.36, "10M SSB QRP", true, 28.360001, 28.679999, "10M All Modes", true, 28.68, 28.68,
			"10M SSTV", true, 28.680001, 29.199999, "10M All Modes", true, 29.2, 29.299999, "10M FM Digital", true,
			29.3, 29.509999, "10M FM Sat. Downlinks", true, 29.51, 29.519999, "10M Guard Channel", true, 29.52, 29.549999,
			"10M FM Simplex", true, 29.55, 29.559999, "10M Deadband", true, 29.56, 29.589999, "10M Repeater Inputs", true,
			29.59, 29.599999, "10M Deadband", true, 29.6, 29.6, "10M FM Calling", true, 29.600001, 29.609999,
			"10M Deadband", true, 29.61, 29.649999, "10M FM Simplex", true, 29.65, 29.659999, "10M Deadband", true,
			29.66, 29.699999, "10M Repeater Outputs", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIndiaBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[92]
		{
			50.0, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.089999, "6M CW", true, 50.09, 50.09,
			"6M CW Calling", true, 50.090001, 50.099999, "6M CW", true, 50.1, 50.109999, "6M CW & SSB", true,
			50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.129999, "6M CW, SSB & Digital", true, 50.13, 50.149999,
			"6M CW, SSB & Digital", true, 50.15, 50.15, "6M SSB Calling", true, 50.150001, 50.249999, "6M CW, SSB & Digital", true,
			50.25, 50.25, "6M PSK Calling", true, 50.250001, 50.499999, "6M CW, SSB & Digital", true, 50.5, 50.619999,
			"6M All Modes", true, 50.62, 50.749999, "6M Digital Comms.", true, 50.75, 51.209999, "6M All Modes", true,
			51.21, 51.389999, "6M FM Repeater Inputs", true, 51.39, 51.409999, "6M All Modes", true, 51.41, 51.509999,
			"6M FM Simplex", true, 51.51, 51.51, "6M FM Calling", true, 51.510001, 51.589999, "6M FM Simplex", true,
			51.59, 51.809999, "6M All Modes", true, 51.81, 51.989999, "6M FM Repeater Ouputs", true, 51.99, 53.999999,
			"6M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[16]
		{
			1.81, 1.837999, "160M CW", true, 1.838, 1.839999, "160M Digital/Packet/CW", true, 1.84, 1.842999,
			"160M Digital/Packet/CW/SSB", true, 1.843, 1.999999, "160M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText80m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[44]
		{
			3.5, 3.509999, "80M CW DX", true, 3.51, 3.56, "80M CW Contest", true, 3.560001, 3.579999,
			"80M CW", true, 3.58, 3.589999, "80M Digital/CW", true, 3.59, 3.599999, "80M Digital/Packet/CW", true,
			3.6, 3.62, "80M SSB/Digital/CW", true, 3.620001, 3.649999, "80M SSB/SSB Contest/CW", true, 3.65, 3.7,
			"80M SSB/CW", true, 3.700001, 3.73, "80M SSB/SSB Contest/CW", true, 3.730001, 3.74, "80M SSTV/FAX/SSB/CW", true,
			3.740001, 3.799999, "80M SSB DX/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText60m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 5.1, 5.499999, "60M General RX", false };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText40m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[16]
		{
			7.0, 7.034999, "40M CW", true, 7.035, 7.04, "40M Digital/SSTV/FAX/CW", true, 7.040001, 7.044999,
			"40M Digital/SSTV/FAX/CW/SSB", true, 7.045, 7.199999, "40M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText30m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[8] { 10.1, 10.139999, "30M CW", true, 10.14, 10.149999, "30M Digital/CW", true };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText20m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[44]
		{
			14.0, 14.059999, "20M CW Contest", true, 14.06, 14.069999, "20M CW", true, 14.07, 14.088999,
			"20M Digital/CW", true, 14.089, 14.098999, "20M Digital(Non-Auto Packet)/CW", true, 14.099, 14.100999, "20M Beacons", true,
			14.101, 14.111999, "20M Digital/SSB/CW", true, 14.112, 14.125, "20M SSB/CW", true, 14.125001, 14.229999,
			"20M SSB Contest/CW", true, 14.23, 14.23, "20M SSTV/FAX Calling", true, 14.230001, 14.299999, "20M SSB Contest/CW", true,
			14.3, 14.349999, "20M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText17m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[16]
		{
			18.068, 18.099999, "17M CW", true, 18.1, 18.108999, "17M Digital/CW", true, 18.109, 18.110999,
			"17M Beacons", true, 18.111, 18.167999, "17M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText15m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[32]
		{
			21.0, 21.079999, "15M CW", true, 21.08, 21.1, "15M Digital/CW", true, 21.100001, 21.119999,
			"15M Packet/Digital/CW", true, 21.12, 21.148999, "15M CW", true, 21.149, 21.150999, "15M Beacons", true,
			21.151, 21.339999, "15M SSB/CW", true, 21.34, 21.34, "15M SSTV/FAX Calling", true, 21.340001, 21.449999,
			"15M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText12m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[16]
		{
			24.89, 24.919999, "12M CW", true, 24.92, 24.928999, "12M Digital/CW", true, 24.929, 24.930999,
			"12M Beacons", true, 24.931, 24.989999, "12M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText10m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[52]
		{
			28.0, 28.049999, "10M CW", true, 28.05, 28.12, "10M Digital/CW", true, 28.120001, 28.149999,
			"10M Digital/Packet/CW", true, 28.15, 28.189999, "10M CW", true, 28.19, 28.198999, "10M Regional Beacons", true,
			28.199, 28.200999, "10M World Wide Beacons", true, 28.201, 28.224999, "10M Continous-Duty Beacons", true, 28.225, 28.679999,
			"10M SSB/CW", true, 28.68, 28.68, "10M SSTV/FAX Calling", true, 28.680001, 29.199999, "10M SSB/CW", true,
			29.2, 29.299999, "10M NBFM Digital/Packet", true, 29.3, 29.509999, "10M Sat. Downlinks", true, 29.51, 29.699999,
			"10M SSB/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegionIsraelBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[36]
		{
			50.0, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.089999, "6M CW", true, 50.09, 50.09,
			"6M CW Calling", true, 50.090001, 50.099999, "6M CW", true, 50.1, 50.109999, "6M CW/SSB", true,
			50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.149999, "6M SSB/Digital/CW", true, 50.15, 50.15,
			"6M SSB Calling", true, 50.150001, 50.199999, "6M SSB/Digital/CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandText4m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[68]
		{
			70.0, 70.089999, "4M WSPR Beacons", true, 70.09, 70.099999, "4M Beacons", true, 70.1, 70.249999,
			"4M CW & SSB", true, 70.25, 70.25, "4M CW & SSB Calling", true, 70.250001, 70.259999, "4M AM & FM", true,
			70.26, 70.26, "4M AM & FM Calling", true, 70.260001, 70.299999, "4M AM & FM", true, 70.3, 70.3,
			"4M RTTY & FAX", true, 70.300001, 70.449999, "4M FM Channels", true, 70.45, 70.45, "4M FM Calling", true,
			70.450001, 70.462499, "4M FM Channels", true, 70.4625, 70.4625, "4M FM Calling", true, 70.462501, 70.474999,
			"4M FM Channels", true, 70.475, 70.475, "4M FM Calling", true, 70.475001, 70.487499, "4M FM Channels", true,
			70.4875, 70.4875, "4M FM Digital", true, 70.487501, 70.499999, "4M FM Channels", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion1BandTextVHFplus()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[704]
		{
			144.0, 144.034999, "2M CW & SSB EME", true, 144.035, 144.049999, "2M CW", true, 144.05, 144.05,
			"2M CW Calling", true, 144.050001, 144.149999, "2M CW", true, 144.15, 144.299999, "2M SSB", true,
			144.3, 144.3, "2M SSB Calling", true, 144.300001, 144.399999, "2M SSB", true, 144.4, 144.489999,
			"2M Beacons", true, 144.49, 144.499999, "2M Guard Band", true, 144.5, 144.799999, "2M All Mode", true,
			144.8, 144.989999, "2M Digital", true, 144.99, 144.993999, "2M Deadband", true, 144.994, 145.193499,
			"2M Repeater Inputs", true, 145.1935, 145.193999, "2M Deadband", true, 145.194, 145.499999, "2M FM Simplex", true,
			145.5, 145.5, "2M FM Calling", true, 145.500001, 145.593499, "2M FM Simplex", true, 145.5935, 145.593999,
			"2M Deadband", true, 145.594, 145.793499, "2M Repeater Outputs", true, 145.7935, 145.799999, "2M Deadband", true,
			145.8, 146.0, "2M All Mode Sat.", true, 430.0, 430.024999, "70cm Sub-Regional", true, 430.025, 430.374999,
			"70cm Repeater Outputs", true, 430.375, 430.399999, "70cm Sub-Regional", true, 430.4, 430.574999, "70cm FM Digital Link", true,
			430.575, 430.599999, "70cm Sub-Regional", true, 430.6, 430.924999, "70cm FM Digital Repeater", true, 430.925, 431.024999,
			"70cm Multimode Channels", true, 431.025, 431.049999, "70cm Sub-Regional", true, 431.05, 431.974999, "70cm Repeater Inputs", true,
			431.975, 431.999999, "70cm Sub-Regional", true, 432.0, 432.024999, "70cm CW EME", true, 432.025, 432.049999,
			"70cm CW", true, 432.05, 432.05, "70cm CW Calling", true, 432.050001, 432.149999, "70cm CW", true,
			432.15, 432.199999, "70cm CW & SSB", true, 432.2, 432.2, "70cm SSB Calling", true, 432.200001, 432.499999,
			"70cm CW & SSB", true, 432.5, 432.5, "70cm SSTV", true, 432.500001, 432.599999, "70cm Transponder Input", true,
			432.6, 432.6, "70cm Digital", true, 432.600001, 432.609999, "70cm Transponder Output", true, 432.61, 432.61,
			"70cm PSK", true, 432.610001, 432.699999, "70cm Transponder Output", true, 432.7, 432.7, "70cm PSK", true,
			432.700001, 432.799999, "70cm Transponder Output", true, 432.8, 432.989999, "70cm Beacons", true, 432.99, 432.993999,
			"70cm Deadband", true, 432.994, 433.380999, "70cm Repeater Input", true, 433.381, 433.393999, "70cm Deadband", true,
			433.394, 433.399999, "70cm FM Simplex", true, 433.4, 433.4, "70cm FM SSTV", true, 433.400001, 433.499999,
			"70cm FM Simplex", true, 433.5, 433.5, "70cm FM Calling", true, 433.500001, 433.580999, "70cm FM Simplex", true,
			433.581, 433.599999, "70cm Deadband", true, 433.6, 433.624999, "70cm All Mode", true, 433.625, 433.774999,
			"70cm Digital Modes", true, 433.775, 433.999999, "70cm All Mode", true, 434.0, 434.449999, "70cm ATV", true,
			434.45, 434.474999, "70cm Digital Comms", true, 434.475, 434.593999, "70cm ATV", true, 434.594, 434.980999,
			"70cm ATV & Repeater Output", true, 434.981, 437.999999, "70cm ATV & Satellite", true, 438.0, 438.024999, "70cm ATV & Sub-Regional", true,
			438.025, 438.174999, "70cm Digital Comms", true, 438.175, 438.199999, "70cm ATV & Sub-Regional", true, 438.2, 438.524999,
			"70cm Digital Repeater", true, 438.525, 438.549999, "70cm ATV & Sub-Regional", true, 438.55, 438.624999, "70cm multi-mode Channels", true,
			438.625, 438.649999, "70cm ATV & Sub-Regional", true, 438.65, 439.424999, "70cm Repeater Output", true, 439.425, 439.799999,
			"70cm ATV & Sub-Regional", true, 439.8, 439.974999, "70cm Digital Comm. Link", true, 439.975, 440.0, "70cm ATV & Sub-Regional", true,
			1240.0, 1240.999999, "23cm All Modes, Digital", true, 1241.0, 1242.024999, "23cm All Modes", true, 1242.025, 1242.699999,
			"23cm Repeater Output", true, 1242.7, 1242.724999, "23cm All Modes", true, 1242.725, 1243.249999, "23cm Packet", true,
			1243.25, 1258.149999, "23cm ATV", true, 1258.15, 1259.349999, "23cm Repeater Output", true, 1259.35, 1259.999999,
			"23cm ATV", true, 1260.0, 1269.999999, "23cm Satellite", true, 1270.0, 1270.024999, "23cm All Modes", true,
			1270.025, 1270.699999, "23cm Repeater Input", true, 1270.7, 1270.724999, "23cm All Modes", true, 1270.725, 1271.249999,
			"23cm Packet", true, 1271.25, 1271.999999, "23cm All Modes", true, 1272.0, 1290.993999, "23cm ATV", true,
			1290.994, 1291.480999, "23cm NBFM Repeater Input", true, 1291.481, 1291.493999, "23cm Deadband", true, 1291.494, 1293.149999,
			"23cm All Modes", true, 1293.15, 1293.349999, "23cm Repeater Input", true, 1293.35, 1295.999999, "23cm All Modes", true,
			1296.0, 1296.024999, "23cm CW EME", true, 1296.025, 1296.149999, "23cm CW", true, 1296.15, 1296.199999,
			"23cm CW & SSB", true, 1296.2, 1296.2, "23cm CW Calling", true, 1296.200001, 1296.399999, "23cm CW & SSB", true,
			1296.4, 1296.499999, "23cm Transponder Input", true, 1296.5, 1296.5, "23cm SSTV", true, 1296.500001, 1296.599999,
			"23cm Transponder Input", true, 1296.6, 1296.6, "23cm RTTY", true, 1296.600001, 1296.699999, "23cm Transponder Output", true,
			1296.7, 1296.7, "23cm Digital", true, 1296.700001, 1296.799999, "23cm Transponder Output", true, 1296.8, 1296.993999,
			"23cm Beacons", true, 1296.994, 1297.480999, "23cm NBFM Repeater Output", true, 1297.481, 1297.493999, "23cm Deadband", true,
			1297.494, 1297.980999, "23cm NBFM Simplex", true, 1297.981, 1297.999999, "23cm Deadband", true, 1298.0, 1298.024999,
			"23cm All Modes", true, 1298.025, 1298.499999, "23cm Repeater Output", true, 1298.5, 1298.724999, "23cm All Modes Digital", true,
			1298.725, 1298.999999, "23cm All Modes Packet", true, 1299.0, 1300.0, "23cm All Modes Digital", true, 2300.0, 2303.999999,
			"13cm Sub-Regional", true, 2304.0, 2305.999999, "13cm Narrow Band ", true, 2306.0, 2307.999999, "13cm Sub-Regional", true,
			2308.0, 2309.999999, "13cm Narrow Band ", true, 2310.0, 2319.999999, "13cm Sub-Regional", true, 2320.0, 2320.024999,
			"13cm CW EME", true, 2320.025, 2320.149999, "13cm CW", true, 2320.15, 2320.199999, "13cm CW & SSB", true,
			2320.2, 2320.2, "13cm SSB Calling", true, 2320.200001, 2320.799999, "13cm CW & SSB", true, 2320.8, 2320.999999,
			"13cm Beacons", true, 2321.0, 2321.999999, "13cm NBFM Simplex", true, 2322.0, 2354.999999, "13cm ATV", true,
			2355.0, 2364.999999, "13cm Digital Comms", true, 2365.0, 2369.999999, "13cm Repeaters", true, 2370.0, 2391.999999,
			"13cm ATV", true, 2392.0, 2399.999999, "13cm Digital Comms", true, 2400.0, 2450.0, "13cm Satellite", true,
			3400.0, 3400.099999, "9cm Narrow Band Modes", true, 3400.1, 3400.1, "9cm Narrow Band Calling", true, 3400.100001, 3401.999999,
			"9cm Narrow Band Modes", true, 3402.0, 3419.999999, "9cm All Modes", true, 3420.0, 3429.999999, "9cm All Modes Digital", true,
			3430.0, 3449.999999, "9cm All Modes", true, 3450.0, 3454.999999, "9cm All Modes Digital", true, 3455.0, 3475.0,
			"9cm All Modes", true, 5650.0, 5667.999999, "5cm Satellite Uplink", true, 5668.0, 5668.199999, "5cm Sat Uplink/Narrow Band", true,
			5668.2, 5668.2, "5cm Narrow Band calling", true, 5668.200001, 5669.999999, "5cm Sat Uplink/Narrow Band", true, 5670.0, 5699.999999,
			"5cm Digital", true, 5700.0, 5719.999999, "5cm ATV", true, 5720.0, 5759.999999, "5cm All Modes", true,
			5760.0, 5760.199999, "5cm Narrow Band Modes", true, 5760.2, 5760.2, "5cm Narrow Band Calling", true, 5760.200001, 5761.999999,
			"5cm Narrow Band Modes", true, 5762.0, 5789.999999, "5cm All Modes", true, 5790.0, 5850.0, "5cm Satellite Downlink", true,
			10000.0, 10149.999999, "3cm Digital", true, 10150.0, 10249.999999, "3cm All Modes", true, 10250.0, 10349.999999,
			"3cm Digital", true, 10350.0, 10367.999999, "3cm All Modes", true, 10368.0, 10368.199999, "3cm Narrow Band Modes", true,
			10368.2, 10368.2, "3cm Narrow Band Calling", true, 10368.200001, 10369.999999, "3cm Narrow Band Modes", true, 10370.0, 10449.999999,
			"3cm All Modes", true, 10450.0, 10500.0, "3cm Satellite/All Modes", true, 24000.0, 24047.999999, "1.2cm Satellite", true,
			24048.0, 24048.199999, "1.2cm Narrow Band Modes", true, 24048.2, 24048.2, "1.2cm Narrow Band Calling", true, 24048.200001, 24049.999999,
			"1.2cm Narrow Band", true, 24050.0, 24191.999999, "1.2cm All Modes", true, 24192.0, 24191.199999, "1.2cm All Modes", true,
			24192.2, 24192.2, "1.2cm Narrow Band Calling", true, 24192.200001, 24193.999999, "1.2cm Narrow Band", true, 24194.0, 24250.0,
			"1.2cm All Modes", true, 47000.0, 47087.999999, "6mm All Mode", true, 47088.0, 47088.0, "6mm Narrow Band Calling", true,
			47088.000001, 47200.0, "6mm All Mode", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddBulgariaBandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[24]
		{
			1.81, 1.835999, "160M CW", true, 1.836, 1.836, "160M CW QRP", true, 1.836001, 1.837999,
			"160M CW", true, 1.838, 1.839999, "160M Narrow Band Modes", true, 1.84, 1.849999, "160M All Modes & Digital", true,
			1.85, 1.999999, "160M General RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddNetherlandsBandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[24]
		{
			1.81, 1.835999, "160M CW", true, 1.836, 1.836, "160M CW QRP", true, 1.836001, 1.837999,
			"160M CW", true, 1.838, 1.839999, "160M Narrow Band Modes", true, 1.84, 1.879999, "160M All Modes & Digital", true,
			1.88, 1.999999, "160M General RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddUK_PlusBandText60m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[92]
		{
			5.1, 5.258499, "60M Band", false, 5.2585, 5.263999, "60M Band Segment 1", true, 5.264, 5.275999,
			"60M Band", false, 5.276, 5.283999, "60M Band Segment 2", true, 5.284, 5.288499, "60M Band", false,
			5.2885, 5.291999, "60M Band Segment 3", true, 5.292, 5.297999, "60M Band", false, 5.298, 5.306999,
			"60M Band Segment 4", true, 5.307, 5.312999, "60M Band", false, 5.313, 5.322999, "60M Band Segment 5", true,
			5.323, 5.332999, "60M Band", false, 5.333, 5.337999, "60M Band Segment 6", true, 5.338, 5.353999,
			"60M Band", false, 5.354, 5.357999, "60M Band Segment 7", true, 5.358, 5.361999, "60M Band", false,
			5.362, 5.374499, "60M Band Segment 8", true, 5.3745, 5.377999, "60M Band", false, 5.378, 5.381999,
			"60M Band Segment 9", true, 5.382, 5.394999, "60M Band", false, 5.395, 5.401499, "60M Band Segment 10", true,
			5.4015, 5.403499, "60M Band", false, 5.4035, 5.406499, "60M Band Segment 11", true, 5.4065, 5.499999,
			"60M Band", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddNorwayBandText60m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 5.25, 5.449999, "60M Amateur Service", true };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddSwedenBandText60m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[36]
		{
			5.25, 5.309999, "60M Band", false, 5.31, 5.313, "60M Band Segment 1", true, 5.313001, 5.319999,
			"60M Band", false, 5.32, 5.323, "60M Band Segment 2", true, 5.323001, 5.379999, "60M Band", false,
			5.38, 5.383, "60M Band Segment 3", true, 5.383001, 5.389999, "60M Band", false, 5.39, 5.393,
			"60M Band Segment 4", true, 5.393001, 5.449999, "60M Band", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddHungaryBandText40m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[40]
		{
			7.0, 7.029999, "40M CW", true, 7.03, 7.03, "40M CW QRP", true, 7.030001, 7.034999,
			"40M CW", true, 7.035, 7.039999, "40M Narrow Band Modes", true, 7.04, 7.059999, "40M All Modes", true,
			7.06, 7.06, "40M SSB Emergency", true, 7.060001, 7.089999, "40M All Modes", true, 7.09, 7.09,
			"40M SSB QRP", true, 7.090001, 7.099999, "40M All Modes", true, 7.1, 7.199999, "40M General RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRussiaBandText12m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[28]
		{
			24.89, 24.905999, "12M CW", true, 24.906, 24.906, "12M CW QRP", true, 24.906001, 24.914999,
			"12M CW", true, 24.915, 24.928999, "12M Narrow Band Modes", true, 24.929, 24.930999, "12M Beacons", true,
			24.931, 24.939999, "12M All Modes Digital", true, 24.94, 25.139999, "12M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRussiaBandText11m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 26.97, 27.85, "11M Citizens Band", true };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddEUBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[64]
		{
			50.0, 50.079999, "6M Beacon Sub-Band", false, 50.08, 50.089999, "6M CW", true, 50.09, 50.09,
			"6M CW Calling", true, 50.090001, 50.099999, "6M CW", true, 50.1, 50.109999, "6M CW & SSB", true,
			50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.129999, "6M CW, SSB & Digital", true, 50.13, 50.149999,
			"6M CW, SSB & Digital", true, 50.15, 50.15, "6M SSB Calling", true, 50.150001, 50.249999, "6M CW, SSB & Digital", true,
			50.25, 50.25, "6M PSK Calling", true, 50.250001, 50.499999, "6M CW, SSB & Digital", true, 50.5, 50.619999,
			"6M All Modes", true, 50.62, 50.749999, "6M Digital Comms.", true, 50.75, 50.999999, "6M All Modes", true,
			51.0, 51.999999, "6M General RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddFranceBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[64]
		{
			50.0, 50.079999, "6M Beacon Sub-Band", false, 50.08, 50.089999, "6M CW", true, 50.09, 50.09,
			"6M CW Calling", true, 50.090001, 50.099999, "6M CW", true, 50.1, 50.109999, "6M CW & SSB", true,
			50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.129999, "6M CW, SSB & Digital", true, 50.13, 50.149999,
			"6M CW, SSB & Digital", true, 50.15, 50.15, "6M SSB Calling", true, 50.150001, 50.249999, "6M CW, SSB & Digital", true,
			50.25, 50.25, "6M PSK Calling", true, 50.250001, 50.499999, "6M CW, SSB & Digital", true, 50.5, 50.619999,
			"6M All Modes", true, 50.62, 50.749999, "6M Digital Comms.", true, 50.75, 51.199999, "6M All Modes", true,
			51.2, 51.999999, "6M General RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddLatviaBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[64]
		{
			50.0, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.089999, "6M CW", true, 50.09, 50.09,
			"6M CW Calling", true, 50.090001, 50.099999, "6M CW", true, 50.1, 50.109999, "6M CW & SSB", true,
			50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.129999, "6M CW, SSB & Digital", true, 50.13, 50.149999,
			"6M CW, SSB & Digital", true, 50.15, 50.15, "6M SSB Calling", true, 50.150001, 50.249999, "6M CW, SSB & Digital", true,
			50.25, 50.25, "6M PSK Calling", true, 50.250001, 50.499999, "6M CW, SSB & Digital", true, 50.5, 50.619999,
			"6M All Modes", true, 50.62, 50.749999, "6M Digital Comms.", true, 50.75, 50.999999, "6M All Modes", true,
			51.0, 51.999999, "6M General RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddBulgariaBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[64]
		{
			50.0, 50.049999, "6M Beacon Sub-Band RX", false, 50.05, 50.079999, "6M Beacon Sub-Band", true, 50.08, 50.089999,
			"6M CW", true, 50.09, 50.09, "6M CW Calling", true, 50.090001, 50.099999, "6M CW", true,
			50.1, 50.109999, "6M CW & SSB", true, 50.11, 50.11, "6M SSB DX Calling", true, 50.110001, 50.129999,
			"6M CW, SSB & Digital", true, 50.13, 50.149999, "6M CW, SSB & Digital", true, 50.15, 50.15, "6M SSB Calling", true,
			50.150001, 50.249999, "6M CW, SSB & Digital", true, 50.25, 50.25, "6M PSK Calling", true, 50.250001, 50.499999,
			"6M CW, SSB & Digital", true, 50.5, 50.619999, "6M All Modes", true, 50.62, 50.749999, "6M Digital Comms.", true,
			50.75, 51.999999, "6M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddGreeceBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 50.0, 51.999999, "6M General RX", false };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[16]
		{
			1.8, 1.829999, "160M CW", true, 1.83, 1.833999, "160M CW & NB Digital", true, 1.834, 1.839999,
			"160M CW", true, 1.84, 1.999999, "160M CW & Phone", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText80m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[28]
		{
			3.5, 3.509999, "80M CW DX", true, 3.51, 3.534999, "80M CW", true, 3.535, 3.599999,
			"80M Phone & CW", true, 3.6, 3.6, "80M IARU Emergency", true, 3.600001, 3.774999, "80M Phone & CW", true,
			3.775, 3.799999, "80M DX Phone & CW", true, 3.8, 3.899999, "80M Phone & CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText60m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 5.1, 5.449999, "60M RX Only", false };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText40m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[24]
		{
			7.0, 7.024999, "40M CW", true, 7.025, 7.029999, "40M CW & NB Digital", true, 7.03, 7.039999,
			"40M All Modes", true, 7.04, 7.109999, "40M Phone & CW", true, 7.11, 7.11, "40M IARU Emergency", true,
			7.110001, 7.299999, "40M Phone & CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText30m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[8] { 10.1, 10.139999, "30M CW", true, 10.14, 10.149999, "30M CW & NB Digital", true };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText20m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[48]
		{
			14.0, 14.069999, "20M CW", true, 14.07, 14.094999, "20M CW & NB Digital", true, 14.095, 14.099499,
			"20M Data & Packet", true, 14.0995, 14.099999, "20M Beacons", true, 14.1, 14.1, "20M NCDXF Beacons", true,
			14.100001, 14.100499, "20M Beacons", true, 14.1005, 14.111999, "20M Data & Packet", true, 14.112, 14.229999,
			"20M Phone & CW", true, 14.23, 14.23, "20M SSTV", true, 14.230001, 14.299999, "20M Phone & CW", true,
			14.3, 14.3, "20M IARU Emergency", true, 14.300001, 14.349999, "20M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText17m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[32]
		{
			18.068, 18.099999, "17M CW", true, 18.1, 18.109499, "17M CW & NB Digital", true, 18.1095, 18.109999,
			"17M Beacons", true, 18.11, 18.11, "17M NCDXF Beacons", true, 18.110001, 18.110499, "17M Beacons", true,
			18.1105, 18.159999, "17M Phone & CW", true, 18.16, 18.16, "17M IARU Emergency", true, 18.160001, 18.167999,
			"17M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText15m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[44]
		{
			21.0, 21.069999, "15M CW", true, 21.07, 21.124999, "15M CW & NB Digital", true, 21.125, 21.149499,
			"15M Phone & CW", true, 21.1495, 21.149999, "15M Beacons", true, 21.15, 21.15, "15M NCDXF Beacons", true,
			21.150001, 21.150499, "15M Beacons", true, 21.1505, 21.339999, "15M Phone & CW", true, 21.34, 21.34,
			"15M SSTV", true, 21.340001, 21.359999, "15M Phone & CW", true, 21.36, 21.36, "15M Emergency", true,
			21.360001, 21.449999, "15M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText12m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[24]
		{
			24.89, 24.919999, "12M CW", true, 24.92, 24.929499, "12M CW & NB Digital", true, 24.9295, 24.929999,
			"12M Beacons", true, 24.93, 24.93, "12M NCDXF Beacons", true, 24.930001, 24.930499, "12M Beacons", true,
			24.9305, 24.989999, "12M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText10m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[44]
		{
			28.0, 28.049999, "10M CW", true, 28.05, 28.149999, "10M CW & NB Digital", true, 28.15, 28.189999,
			"10M CW", true, 28.19, 28.199999, "10M Beacons", true, 28.2, 28.2, "10M NCDXF Beacons", true,
			28.200001, 28.200499, "10M Beacons", true, 28.2005, 28.679999, "10M Phone & CW", true, 28.68, 28.68,
			"10M SSTV", true, 28.680001, 29.299999, "10M Phone & CW", true, 29.3, 29.509999, "10M Satellite & CW", true,
			29.51, 29.699999, "10M Wide Band  & CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[12]
		{
			50.0, 50.099999, "6M CW & Beacons", true, 50.1, 50.499999, "6M Phone/NB Digital/CW", true, 50.5, 53.999999,
			"6M Wide Band Modes & CW", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddRegion3BandTextVHFplus()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[356]
		{
			144.0, 144.019999, "2M EME", true, 144.02, 144.099999, "2M CW & EME", true, 144.1, 144.399999,
			"2M CW/Phone & Image", true, 144.4, 144.499999, "2M CW/Phone/NB Digital", true, 144.5, 144.699999, "2M Wide Digital Modes", true,
			144.7, 145.499999, "2M FM, CW & Image", true, 145.5, 145.5, "2M Emergency", true, 145.500001, 145.649999,
			"2M FM, CW & Image", true, 145.65, 145.799999, "2M All Modes", true, 145.8, 145.999999, "2M Satellite.", true,
			146.0, 147.999999, "2M All Modes", true, 430.0, 430.099999, "70cm CW", true, 430.1, 430.699999,
			"70cm CW/Phone & Image", true, 430.7, 430.799999, "70cm CW/Phone/NB Digital", true, 430.8, 431.399999, "70cm Wide Digital Modes", true,
			431.4, 431.899999, "70cm FM, CW & Image", true, 431.9, 432.099999, "70cm EME", true, 432.1, 432.999999,
			"70cm FM, CW & Image", true, 433.0, 433.0, "70cm Emergency", true, 433.000001, 433.999999, "70cm FM, CW & Image", true,
			434.0, 434.999999, "70cm Repeaters", true, 435.0, 437.999999, "70cm Satellite", true, 438.0, 438.999999,
			"70cm All Modes", true, 439.0, 439.999999, "70cm Repeaters", true, 440.0, 449.999999, "70cm All Modes", true,
			1240.0, 1259.999999, "23cm All Modes", true, 1260.0, 1269.999999, "23cm Satellite", true, 1270.0, 1295.999999,
			"23cm All Modes", true, 1296.7, 1296.999999, "23cm EME - all modes", true, 1297.725, 1299.999999, "23cm All Modes", true,
			2300.0, 2303.999999, "13cm Sub-Regional", true, 2304.0, 2305.999999, "13cm Narrow Band ", true, 2306.0, 2307.999999,
			"13cm Sub-Regional", true, 2308.0, 2309.999999, "13cm Narrow Band ", true, 2310.0, 2319.999999, "13cm Sub-Regional", true,
			2320.0, 2320.024999, "13cm CW EME", true, 2320.025, 2320.149999, "13cm CW", true, 2320.15, 2320.199999,
			"13cm CW & SSB", true, 2320.2, 2320.2, "13cm SSB Calling", true, 2320.200001, 2320.799999, "13cm CW & SSB", true,
			2320.8, 2320.999999, "13cm Beacons", true, 2321.0, 2321.999999, "13cm NBFM Simplex", true, 2322.0, 2354.999999,
			"13cm ATV", true, 2355.0, 2364.999999, "13cm Digital Comms", true, 2365.0, 2369.999999, "13cm Repeaters", true,
			2370.0, 2391.999999, "13cm ATV", true, 2392.0, 2399.999999, "13cm Digital Comms", true, 2400.0, 2450.0,
			"13cm Satellite", true, 3400.0, 3400.099999, "9cm Narrow Band Modes", true, 3400.1, 3400.1, "9cm Narrow Band Calling", true,
			3400.100001, 3401.999999, "9cm Narrow Band Modes", true, 3402.0, 3419.999999, "9cm All Modes", true, 3420.0, 3429.999999,
			"9cm All Modes Digital", true, 3430.0, 3449.999999, "9cm All Modes", true, 3450.0, 3454.999999, "9cm All Modes Digital", true,
			3455.0, 3475.0, "9cm All Modes", true, 5650.0, 5667.999999, "5cm Satellite Uplink", true, 5668.0, 5668.199999,
			"5cm Sat Uplink/Narrow Band", true, 5668.2, 5668.2, "5cm Narrow Band calling", true, 5668.200001, 5669.999999, "5cm Sat Uplink/Narrow Band", true,
			5670.0, 5699.999999, "5cm Digital", true, 5700.0, 5719.999999, "5cm ATV", true, 5720.0, 5759.999999,
			"5cm All Modes", true, 5760.0, 5760.199999, "5cm Narrow Band Modes", true, 5760.2, 5760.2, "5cm Narrow Band Calling", true,
			5760.200001, 5761.999999, "5cm Narrow Band Modes", true, 5762.0, 5789.999999, "5cm All Modes", true, 5790.0, 5850.0,
			"5cm Satellite Downlink", true, 10000.0, 10149.999999, "3cm Digital", true, 10150.0, 10249.999999, "3cm All Modes", true,
			10250.0, 10349.999999, "3cm Digital", true, 10350.0, 10367.999999, "3cm All Modes", true, 10368.0, 10368.199999,
			"3cm Narrow Band Modes", true, 10368.2, 10368.2, "3cm Narrow Band Calling", true, 10368.200001, 10369.999999, "3cm Narrow Band Modes", true,
			10370.0, 10449.999999, "3cm All Modes", true, 10450.0, 10500.0, "3cm Satellite/All Modes", true, 24000.0, 24047.999999,
			"1.2cm Satellite", true, 24048.0, 24048.199999, "1.2cm Narrow Band Modes", true, 24048.2, 24048.2, "1.2cm Narrow Band Calling", true,
			24048.200001, 24049.999999, "1.2cm Narrow Band", true, 24050.0, 24191.999999, "1.2cm All Modes", true, 24192.0, 24191.199999,
			"1.2cm All Modes", true, 24192.2, 24192.2, "1.2cm Narrow Band Calling", true, 24192.200001, 24193.999999, "1.2cm Narrow Band", true,
			24194.0, 24250.0, "1.2cm All Modes", true, 47000.0, 47087.999999, "6mm All Mode", true, 47088.0, 47088.0,
			"6mm Narrow Band Calling", true, 47088.000001, 47200.0, "6mm All Mode", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddJapanBandText160m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[20]
		{
			1.8, 1.809999, "160M Band RX", false, 1.81, 1.824999, "160M CW", true, 1.825, 1.907499,
			"160M Band RX", false, 1.9075, 1.912499, "160M CW & NB Digital", true, 1.9125, 1.999999, "160M Band RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddJapanBandText80m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[64]
		{
			3.5, 3.519999, "80M CW", true, 3.52, 3.524999, "80M CW & NB Digital", true, 3.525, 3.525,
			"80M Emergency", true, 3.525001, 3.529999, "80M Phone/CW/NB Digital", true, 3.53, 3.574999, "80M Phone/CW/Digital", true,
			3.575, 3.598999, "80M Band RX", false, 3.599, 3.611999, "80M Phone/CW/Digital", true, 3.612, 3.679999,
			"80M Band RX", false, 3.68, 3.686999, "80M Phone/CW/Image", true, 3.687, 3.701999, "80M Band RX", false,
			3.702, 3.715999, "80M Phone/CW/Image", true, 3.716, 3.744999, "80M Band RX", false, 3.745, 3.769999,
			"80M Phone/CW/Image", true, 3.77, 3.790999, "80M Band RX", false, 3.791, 3.804999, "80M Phone/CW/NB Digital", true,
			3.805, 3.899999, "80M Band RX", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddJapanBandText40m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[32]
		{
			7.0, 7.024999, "40M CW", true, 7.025, 7.029999, "40M CW & NB Digital", true, 7.03, 7.03,
			"40M Emergency", true, 7.030001, 7.039999, "40M CW & NB Digital", true, 7.04, 7.044999, "40M DX NB Digital/CW", true,
			7.045, 7.099999, "40M CW/Phone/Image", true, 7.1, 7.199999, "40M All Modes", true, 7.2, 7.299999,
			"40M RX Only", false
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddJapanBandText10m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[48]
		{
			28.0, 28.069999, "10M CW", true, 28.07, 28.149999, "10M CW & NB Digital", true, 28.15, 28.199499,
			"10M CW", true, 28.1995, 28.199999, "10M Beacons", true, 28.2, 28.2, "10M NCDXF Beacons", true,
			28.200001, 28.2005, "10M Beacons", true, 28.200501, 28.999999, "10M Phone/CW/NB Digital", true, 29.0, 29.299999,
			"10M DX Phone/CW/Digital", true, 29.3, 29.509999, "10M Satellite", true, 29.51, 29.589999, "10M Repeater", true,
			29.59, 29.609999, "10M Wide Phone & CW", true, 29.61, 29.699999, "10M Repeater", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddJapanBandText6m()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[28]
		{
			50.0, 50.099999, "6M DX CW/EME/Beacons", true, 50.1, 50.899999, "6M Phone/CW/Image", true, 50.9, 50.999999,
			"6M Phone/CW/NB Digital", true, 51.1, 51.999999, "6M Wide Phone/Image/CW", true, 52.0, 52.499999, "6M Phone/CW/NB Digital", true,
			52.5, 52.899999, "6M Wide Digital Modes", true, 52.9, 53.999999, "6M All Modes", true
		};
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddJapanBandTextEmergency()
	{
		DataTable dataTable = ds.Tables["BandText"];
		object[] array = new object[4] { 4.629995, 4.630005, "Japan Int. Emergency", true };
		for (int i = 0; i < array.Length / 4; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow["Low"] = (double)array[i * 4];
			dataRow["High"] = (double)array[i * 4 + 1];
			dataRow["Name"] = (string)array[i * 4 + 2];
			dataRow["TX"] = (bool)array[i * 4 + 3];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddMemoryTable()
	{
		ds.Tables.Add("Memory");
		DataTable dataTable = ds.Tables["Memory"];
		dataTable.Columns.Add("GroupID", typeof(int));
		dataTable.Columns.Add("Freq", typeof(double));
		dataTable.Columns.Add("ModeID", typeof(int));
		dataTable.Columns.Add("FilterID", typeof(int));
		dataTable.Columns.Add("Callsign", typeof(string));
		dataTable.Columns.Add("Comments", typeof(string));
		dataTable.Columns.Add("Scan", typeof(bool));
		dataTable.Columns.Add("Squelch", typeof(int));
		dataTable.Columns.Add("StepSizeID", typeof(int));
		dataTable.Columns.Add("AGCID", typeof(int));
		dataTable.Columns.Add("Gain", typeof(string));
		dataTable.Columns.Add("FilterLow", typeof(int));
		dataTable.Columns.Add("FilterHigh", typeof(int));
		dataTable.Columns.Add("CreateDate", typeof(string));
	}

	private static void AddGroupListTable()
	{
		ds.Tables.Add("GroupList");
		DataTable dataTable = ds.Tables["GroupList"];
		dataTable.Columns.Add("GroupID", typeof(int));
		dataTable.Columns.Add("GroupName", typeof(string));
		string[] array = new string[7] { "AM", "FM", "SSB", "SSTV", "CW", "PSK", "RTTY" };
		for (int i = 0; i < array.Length; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow[0] = i;
			dataRow[1] = array[i];
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void AddTXProfileTable(string sTableName, bool bIndcludeExtraProfiles = false)
	{
		ds.Tables.Add(sTableName);
		DataTable dataTable = ds.Tables[sTableName];
		dataTable.Columns.Add("Name", typeof(string));
		dataTable.Columns.Add("FilterLow", typeof(int));
		dataTable.Columns.Add("FilterHigh", typeof(int));
		dataTable.Columns.Add("EQUseLegacy", typeof(bool));
		dataTable.Columns.Add("RXParaEQData", typeof(string));
		dataTable.Columns.Add("TXParaEQData", typeof(string));
		dataTable.Columns.Add("TXEQNumBands", typeof(int));
		dataTable.Columns.Add("TXEQEnabled", typeof(bool));
		dataTable.Columns.Add("TXEQPreamp", typeof(int));
		dataTable.Columns.Add("TXEQ1", typeof(int));
		dataTable.Columns.Add("TXEQ2", typeof(int));
		dataTable.Columns.Add("TXEQ3", typeof(int));
		dataTable.Columns.Add("TXEQ4", typeof(int));
		dataTable.Columns.Add("TXEQ5", typeof(int));
		dataTable.Columns.Add("TXEQ6", typeof(int));
		dataTable.Columns.Add("TXEQ7", typeof(int));
		dataTable.Columns.Add("TXEQ8", typeof(int));
		dataTable.Columns.Add("TXEQ9", typeof(int));
		dataTable.Columns.Add("TXEQ10", typeof(int));
		dataTable.Columns.Add("TxEqFreq1", typeof(int));
		dataTable.Columns.Add("TxEqFreq2", typeof(int));
		dataTable.Columns.Add("TxEqFreq3", typeof(int));
		dataTable.Columns.Add("TxEqFreq4", typeof(int));
		dataTable.Columns.Add("TxEqFreq5", typeof(int));
		dataTable.Columns.Add("TxEqFreq6", typeof(int));
		dataTable.Columns.Add("TxEqFreq7", typeof(int));
		dataTable.Columns.Add("TxEqFreq8", typeof(int));
		dataTable.Columns.Add("TxEqFreq9", typeof(int));
		dataTable.Columns.Add("TxEqFreq10", typeof(int));
		dataTable.Columns.Add("DXOn", typeof(bool));
		dataTable.Columns.Add("DXLevel", typeof(int));
		dataTable.Columns.Add("CompanderOn", typeof(bool));
		dataTable.Columns.Add("CompanderLevel", typeof(int));
		dataTable.Columns.Add("MicGain", typeof(int));
		dataTable.Columns.Add("FMMicGain", typeof(int));
		dataTable.Columns.Add("MicMute", typeof(bool));
		dataTable.Columns.Add("Lev_On", typeof(bool));
		dataTable.Columns.Add("Lev_MaxGain", typeof(int));
		dataTable.Columns.Add("Lev_Decay", typeof(int));
		dataTable.Columns.Add("ALC_MaximumGain", typeof(int));
		dataTable.Columns.Add("ALC_Decay", typeof(int));
		dataTable.Columns.Add("Power", typeof(int));
		dataTable.Columns.Add("VOX_On", typeof(bool));
		dataTable.Columns.Add("Dexp_On", typeof(bool));
		dataTable.Columns.Add("Dexp_Threshold", typeof(int));
		dataTable.Columns.Add("Dexp_Attack", typeof(int));
		dataTable.Columns.Add("VOX_HangTime", typeof(int));
		dataTable.Columns.Add("Dexp_Release", typeof(int));
		dataTable.Columns.Add("Dexp_Attenuate", typeof(decimal));
		dataTable.Columns.Add("Dexp_Hysterisis", typeof(decimal));
		dataTable.Columns.Add("Dexp_Tau", typeof(int));
		dataTable.Columns.Add("Dexp_SCF_On", typeof(bool));
		dataTable.Columns.Add("Dexp_SCF_Low", typeof(int));
		dataTable.Columns.Add("Dexp_SCF_High", typeof(int));
		dataTable.Columns.Add("Dexp_LookAhead_On", typeof(bool));
		dataTable.Columns.Add("Dexp_LookAhead", typeof(int));
		dataTable.Columns.Add("Tune_Power", typeof(int));
		dataTable.Columns.Add("Tune_Meter_Type", typeof(string));
		dataTable.Columns.Add("TX_AF_Level", typeof(int));
		dataTable.Columns.Add("AM_Carrier_Level", typeof(int));
		dataTable.Columns.Add("Show_TX_Filter", typeof(bool));
		dataTable.Columns.Add("VAC1_On", typeof(bool));
		dataTable.Columns.Add("VAC1_Auto_On", typeof(bool));
		dataTable.Columns.Add("VAC1_RX_Gain", typeof(int));
		dataTable.Columns.Add("VAC1_TX_Gain", typeof(int));
		dataTable.Columns.Add("VAC1_Stereo_On", typeof(bool));
		dataTable.Columns.Add("VAC1_Sample_Rate", typeof(string));
		dataTable.Columns.Add("VAC1_Buffer_Size", typeof(string));
		dataTable.Columns.Add("VAC1_IQ_Output", typeof(bool));
		dataTable.Columns.Add("VAC1_IQ_Correct", typeof(bool));
		dataTable.Columns.Add("VAC1_PTT_OverRide", typeof(bool));
		dataTable.Columns.Add("VAC1_Combine_Input_Channels", typeof(bool));
		dataTable.Columns.Add("VAC1_Latency_On", typeof(bool));
		dataTable.Columns.Add("VAC1_Latency_Duration", typeof(int));
		dataTable.Columns.Add("VAC1_Latency_RBOut_On", typeof(bool));
		dataTable.Columns.Add("VAC1_Latency_RBOut_Duration", typeof(int));
		dataTable.Columns.Add("VAC1_Latency_PAIn_On", typeof(bool));
		dataTable.Columns.Add("VAC1_Latency_PAIn_Duration", typeof(int));
		dataTable.Columns.Add("VAC1_Latency_PAOut_On", typeof(bool));
		dataTable.Columns.Add("VAC1_Latency_PAOut_Duration", typeof(int));
		dataTable.Columns.Add("VAC1_AudioDriver", typeof(string));
		dataTable.Columns.Add("VAC1_AudioInput", typeof(string));
		dataTable.Columns.Add("VAC1_AudioOutput", typeof(string));
		dataTable.Columns.Add("VAC2_On", typeof(bool));
		dataTable.Columns.Add("VAC2_Auto_On", typeof(bool));
		dataTable.Columns.Add("VAC2_RX_Gain", typeof(int));
		dataTable.Columns.Add("VAC2_TX_Gain", typeof(int));
		dataTable.Columns.Add("VAC2_Stereo_On", typeof(bool));
		dataTable.Columns.Add("VAC2_Sample_Rate", typeof(string));
		dataTable.Columns.Add("VAC2_Buffer_Size", typeof(string));
		dataTable.Columns.Add("VAC2_IQ_Output", typeof(bool));
		dataTable.Columns.Add("VAC2_IQ_Correct", typeof(bool));
		dataTable.Columns.Add("VAC2_Combine_Input_Channels", typeof(bool));
		dataTable.Columns.Add("VAC2_Latency_On", typeof(bool));
		dataTable.Columns.Add("VAC2_Latency_Duration", typeof(int));
		dataTable.Columns.Add("VAC2_Latency_RBOut_On", typeof(bool));
		dataTable.Columns.Add("VAC2_Latency_RBOut_Duration", typeof(int));
		dataTable.Columns.Add("VAC2_Latency_PAIn_On", typeof(bool));
		dataTable.Columns.Add("VAC2_Latency_PAIn_Duration", typeof(int));
		dataTable.Columns.Add("VAC2_Latency_PAOut_On", typeof(bool));
		dataTable.Columns.Add("VAC2_Latency_PAOut_Duration", typeof(int));
		dataTable.Columns.Add("VAC2_AudioDriver", typeof(string));
		dataTable.Columns.Add("VAC2_AudioInput", typeof(string));
		dataTable.Columns.Add("VAC2_AudioOutput", typeof(string));
		dataTable.Columns.Add("Phone_RX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("Phone_TX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("FM_RX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("FM_TX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("Digi_RX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("Digi_TX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("CW_RX_DSP_Buffer", typeof(string));
		dataTable.Columns.Add("Phone_RX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("Phone_TX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("FM_RX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("FM_TX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("Digi_RX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("Digi_TX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("CW_RX_DSP_Filter_Size", typeof(string));
		dataTable.Columns.Add("Phone_RX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("Phone_TX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("FM_RX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("FM_TX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("Digi_RX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("Digi_TX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("CW_RX_DSP_Filter_Type", typeof(string));
		dataTable.Columns.Add("Mic_Input_On", typeof(bool));
		dataTable.Columns.Add("Mic_Input_Boost", typeof(bool));
		dataTable.Columns.Add("Line_Input_On", typeof(bool));
		dataTable.Columns.Add("Line_Input_Level", typeof(decimal));
		dataTable.Columns.Add("CESSB_On", typeof(bool));
		dataTable.Columns.Add("Pure_Signal_Enabled", typeof(bool));
		dataTable.Columns.Add("FM_RX_AFFilter_Low", typeof(decimal));
		dataTable.Columns.Add("FM_RX_AFFilter_High", typeof(decimal));
		dataTable.Columns.Add("FM_TX_AFFilter_Low", typeof(decimal));
		dataTable.Columns.Add("FM_TX_AFFilter_High", typeof(decimal));
		dataTable.Columns.Add("VAC1_Force_In", typeof(bool));
		dataTable.Columns.Add("VAC1_Force_Out", typeof(bool));
		dataTable.Columns.Add("VAC2_Force_In", typeof(bool));
		dataTable.Columns.Add("VAC2_Force_Out", typeof(bool));
		dataTable.Columns.Add("VAC1_SwapIQ", typeof(bool));
		dataTable.Columns.Add("VAC2_SwapIQ", typeof(bool));
		dataTable.Columns.Add("Audio_Disable_Audio_Amp", typeof(bool));
		dataTable.Columns.Add("PA_Profile", typeof(string));
		dataTable.Columns.Add("RXEQEnabled", typeof(bool));
		dataTable.Columns.Add("RXEQPreamp", typeof(int));
		dataTable.Columns.Add("RXEQ1", typeof(int));
		dataTable.Columns.Add("RXEQ2", typeof(int));
		dataTable.Columns.Add("RXEQ3", typeof(int));
		dataTable.Columns.Add("RXEQ4", typeof(int));
		dataTable.Columns.Add("RXEQ5", typeof(int));
		dataTable.Columns.Add("RXEQ6", typeof(int));
		dataTable.Columns.Add("RXEQ7", typeof(int));
		dataTable.Columns.Add("RXEQ8", typeof(int));
		dataTable.Columns.Add("RXEQ9", typeof(int));
		dataTable.Columns.Add("RXEQ10", typeof(int));
		dataTable.Columns.Add("VAC1_Exclusive_In", typeof(bool));
		dataTable.Columns.Add("VAC1_Exclusive_Out", typeof(bool));
		dataTable.Columns.Add("VAC2_Exclusive_In", typeof(bool));
		dataTable.Columns.Add("VAC2_Exclusive_Out", typeof(bool));
		dataTable.Columns.Add("CFCUseLegacy", typeof(bool));
		dataTable.Columns.Add("CFCEnabled", typeof(bool));
		dataTable.Columns.Add("CFCPostEqEnabled", typeof(bool));
		dataTable.Columns.Add("CFCPhaseRotatorEnabled", typeof(bool));
		dataTable.Columns.Add("CFCPhaseReverseEnabled", typeof(bool));
		dataTable.Columns.Add("CFCPhaseRotatorFreq", typeof(int));
		dataTable.Columns.Add("CFCPhaseRotatorStages", typeof(int));
		dataTable.Columns.Add("CFCPhaseRotatorAuto", typeof(bool));
		dataTable.Columns.Add("CFCPreComp", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain", typeof(int));
		dataTable.Columns.Add("CFCPreComp0", typeof(int));
		dataTable.Columns.Add("CFCPreComp1", typeof(int));
		dataTable.Columns.Add("CFCPreComp2", typeof(int));
		dataTable.Columns.Add("CFCPreComp3", typeof(int));
		dataTable.Columns.Add("CFCPreComp4", typeof(int));
		dataTable.Columns.Add("CFCPreComp5", typeof(int));
		dataTable.Columns.Add("CFCPreComp6", typeof(int));
		dataTable.Columns.Add("CFCPreComp7", typeof(int));
		dataTable.Columns.Add("CFCPreComp8", typeof(int));
		dataTable.Columns.Add("CFCPreComp9", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain0", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain1", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain2", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain3", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain4", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain5", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain6", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain7", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain8", typeof(int));
		dataTable.Columns.Add("CFCPostEqGain9", typeof(int));
		dataTable.Columns.Add("CFCEqFreq0", typeof(int));
		dataTable.Columns.Add("CFCEqFreq1", typeof(int));
		dataTable.Columns.Add("CFCEqFreq2", typeof(int));
		dataTable.Columns.Add("CFCEqFreq3", typeof(int));
		dataTable.Columns.Add("CFCEqFreq4", typeof(int));
		dataTable.Columns.Add("CFCEqFreq5", typeof(int));
		dataTable.Columns.Add("CFCEqFreq6", typeof(int));
		dataTable.Columns.Add("CFCEqFreq7", typeof(int));
		dataTable.Columns.Add("CFCEqFreq8", typeof(int));
		dataTable.Columns.Add("CFCEqFreq9", typeof(int));
		dataTable.Columns.Add("CFCParaEQData", typeof(string));
		DataRow dataRow = dataTable.NewRow();
		dataRow["Name"] = "Default";
		dataRow["FilterLow"] = 100;
		dataRow["FilterHigh"] = 3000;
		dataRow["EQUseLegacy"] = true;
		dataRow["RXParaEQData"] = "";
		dataRow["TXParaEQData"] = "";
		dataRow["TXEQNumBands"] = 10;
		dataRow["TXEQEnabled"] = false;
		dataRow["TXEQPreamp"] = 0;
		dataRow["TXEQ1"] = 0;
		dataRow["TXEQ2"] = 0;
		dataRow["TXEQ3"] = 0;
		dataRow["TXEQ4"] = 0;
		dataRow["TXEQ5"] = 0;
		dataRow["TXEQ6"] = 0;
		dataRow["TXEQ7"] = 0;
		dataRow["TXEQ8"] = 0;
		dataRow["TXEQ9"] = 0;
		dataRow["TXEQ10"] = 0;
		dataRow["TxEqFreq1"] = 32;
		dataRow["TxEqFreq2"] = 63;
		dataRow["TxEqFreq3"] = 125;
		dataRow["TxEqFreq4"] = 250;
		dataRow["TxEqFreq5"] = 500;
		dataRow["TxEqFreq6"] = 1000;
		dataRow["TxEqFreq7"] = 2000;
		dataRow["TxEqFreq8"] = 4000;
		dataRow["TxEqFreq9"] = 8000;
		dataRow["TxEqFreq10"] = 16000;
		dataRow["DXOn"] = false;
		dataRow["DXLevel"] = 3;
		dataRow["CompanderOn"] = true;
		dataRow["CompanderLevel"] = 2;
		dataRow["MicGain"] = 10;
		dataRow["FMMicGain"] = 10;
		dataRow["MicMute"] = true;
		dataRow["Lev_On"] = true;
		dataRow["Lev_MaxGain"] = 15;
		dataRow["Lev_Decay"] = 100;
		dataRow["ALC_MaximumGain"] = 3;
		dataRow["ALC_Decay"] = 10;
		dataRow["Power"] = 50;
		dataRow["VOX_On"] = false;
		dataRow["Dexp_On"] = false;
		dataRow["Dexp_Threshold"] = -40;
		dataRow["Dexp_Attack"] = 2;
		dataRow["VOX_HangTime"] = 250;
		dataRow["Dexp_Release"] = 100;
		dataRow["Dexp_Attenuate"] = 10.0;
		dataRow["Dexp_Hysterisis"] = 2.0;
		dataRow["Dexp_Tau"] = 20;
		dataRow["Dexp_SCF_On"] = true;
		dataRow["Dexp_SCF_Low"] = 500;
		dataRow["Dexp_SCF_High"] = 1500;
		dataRow["Dexp_LookAhead_On"] = true;
		dataRow["Dexp_LookAhead"] = 60;
		dataRow["Tune_Power"] = 10;
		dataRow["Tune_Meter_Type"] = "Fwd Pwr";
		dataRow["TX_AF_Level"] = 50;
		dataRow["AM_Carrier_Level"] = 100;
		dataRow["Show_TX_Filter"] = true;
		dataRow["VAC1_On"] = false;
		dataRow["VAC1_Auto_On"] = false;
		dataRow["VAC1_RX_GAIN"] = 0;
		dataRow["VAC1_TX_GAIN"] = 0;
		dataRow["VAC1_Stereo_On"] = false;
		dataRow["VAC1_Sample_Rate"] = "48000";
		dataRow["VAC1_Buffer_Size"] = "2048";
		dataRow["VAC1_IQ_Output"] = false;
		dataRow["VAC1_IQ_Correct"] = true;
		dataRow["VAC1_PTT_OverRide"] = true;
		dataRow["VAC1_Combine_Input_Channels"] = false;
		dataRow["VAC1_Latency_On"] = true;
		dataRow["VAC1_Latency_Duration"] = 120;
		dataRow["VAC1_Latency_RBOut_On"] = true;
		dataRow["VAC1_Latency_RBOut_Duration"] = 120;
		dataRow["VAC1_Latency_PAIn_On"] = true;
		dataRow["VAC1_Latency_PAIn_Duration"] = 120;
		dataRow["VAC1_Latency_PAOut_On"] = true;
		dataRow["VAC1_Latency_PAOut_Duration"] = 120;
		dataRow["VAC1_AudioDriver"] = "";
		dataRow["VAC1_AudioInput"] = "";
		dataRow["VAC1_AudioOutput"] = "";
		dataRow["VAC2_On"] = false;
		dataRow["VAC2_Auto_On"] = false;
		dataRow["VAC2_RX_GAIN"] = 0;
		dataRow["VAC2_TX_GAIN"] = 0;
		dataRow["VAC2_Stereo_On"] = false;
		dataRow["VAC2_Sample_Rate"] = "48000";
		dataRow["VAC2_Buffer_Size"] = "2048";
		dataRow["VAC2_IQ_Output"] = false;
		dataRow["VAC2_IQ_Correct"] = true;
		dataRow["VAC2_Combine_Input_Channels"] = false;
		dataRow["VAC2_Latency_On"] = true;
		dataRow["VAC2_Latency_Duration"] = 120;
		dataRow["VAC2_Latency_RBOut_On"] = true;
		dataRow["VAC2_Latency_RBOut_Duration"] = 120;
		dataRow["VAC2_Latency_PAIn_On"] = true;
		dataRow["VAC2_Latency_PAIn_Duration"] = 120;
		dataRow["VAC2_Latency_PAOut_On"] = true;
		dataRow["VAC2_Latency_PAOut_Duration"] = 120;
		dataRow["VAC2_AudioDriver"] = "";
		dataRow["VAC2_AudioInput"] = "";
		dataRow["VAC2_AudioOutput"] = "";
		dataRow["Phone_RX_DSP_Buffer"] = "64";
		dataRow["Phone_TX_DSP_Buffer"] = "64";
		dataRow["FM_RX_DSP_Buffer"] = "256";
		dataRow["FM_TX_DSP_Buffer"] = "128";
		dataRow["Digi_RX_DSP_Buffer"] = "64";
		dataRow["Digi_TX_DSP_Buffer"] = "64";
		dataRow["CW_RX_DSP_Buffer"] = "64";
		dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
		dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
		dataRow["FM_RX_DSP_Filter_Size"] = "4096";
		dataRow["FM_TX_DSP_Filter_Size"] = "4096";
		dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
		dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
		dataRow["CW_RX_DSP_Filter_Size"] = "4096";
		dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
		dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
		dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Mic_Input_On"] = true;
		dataRow["Mic_Input_Boost"] = true;
		dataRow["Line_Input_On"] = false;
		dataRow["Line_Input_Level"] = 0.0;
		dataRow["CESSB_On"] = false;
		dataRow["Pure_Signal_Enabled"] = false;
		dataRow["FM_RX_AFFilter_Low"] = 300;
		dataRow["FM_RX_AFFilter_High"] = 3000;
		dataRow["FM_TX_AFFilter_Low"] = 300;
		dataRow["FM_TX_AFFilter_High"] = 3000;
		dataRow["VAC1_Force_In"] = false;
		dataRow["VAC1_Force_Out"] = false;
		dataRow["VAC2_Force_In"] = false;
		dataRow["VAC2_Force_Out"] = false;
		dataRow["VAC1_SwapIQ"] = true;
		dataRow["VAC2_SwapIQ"] = true;
		dataRow["Audio_Disable_Audio_Amp"] = false;
		dataRow["PA_Profile"] = "";
		dataRow["RXEQEnabled"] = false;
		dataRow["RXEQPreamp"] = 0;
		dataRow["RXEQ1"] = 0;
		dataRow["RXEQ2"] = 0;
		dataRow["RXEQ3"] = 0;
		dataRow["RXEQ4"] = 0;
		dataRow["RXEQ5"] = 0;
		dataRow["RXEQ6"] = 0;
		dataRow["RXEQ7"] = 0;
		dataRow["RXEQ8"] = 0;
		dataRow["RXEQ9"] = 0;
		dataRow["RXEQ10"] = 0;
		dataRow["VAC1_Exclusive_In"] = false;
		dataRow["VAC1_Exclusive_Out"] = false;
		dataRow["VAC2_Exclusive_In"] = false;
		dataRow["VAC2_Exclusive_Out"] = false;
		dataRow["CFCUseLegacy"] = true;
		dataRow["CFCEnabled"] = false;
		dataRow["CFCPostEqEnabled"] = false;
		dataRow["CFCPhaseRotatorEnabled"] = false;
		dataRow["CFCPhaseReverseEnabled"] = false;
		dataRow["CFCPhaseRotatorFreq"] = 338;
		dataRow["CFCPhaseRotatorStages"] = 8;
		dataRow["CFCPhaseRotatorAuto"] = false;
		dataRow["CFCPreComp"] = 0;
		dataRow["CFCPostEqGain"] = 0;
		dataRow["CFCPreComp0"] = 5;
		dataRow["CFCPreComp1"] = 5;
		dataRow["CFCPreComp2"] = 5;
		dataRow["CFCPreComp3"] = 5;
		dataRow["CFCPreComp4"] = 5;
		dataRow["CFCPreComp5"] = 5;
		dataRow["CFCPreComp6"] = 5;
		dataRow["CFCPreComp7"] = 5;
		dataRow["CFCPreComp8"] = 5;
		dataRow["CFCPreComp9"] = 5;
		dataRow["CFCPostEqGain0"] = 0;
		dataRow["CFCPostEqGain1"] = 0;
		dataRow["CFCPostEqGain2"] = 0;
		dataRow["CFCPostEqGain3"] = 0;
		dataRow["CFCPostEqGain4"] = 0;
		dataRow["CFCPostEqGain5"] = 0;
		dataRow["CFCPostEqGain6"] = 0;
		dataRow["CFCPostEqGain7"] = 0;
		dataRow["CFCPostEqGain8"] = 0;
		dataRow["CFCPostEqGain9"] = 0;
		dataRow["CFCEqFreq0"] = 0;
		dataRow["CFCEqFreq1"] = 125;
		dataRow["CFCEqFreq2"] = 250;
		dataRow["CFCEqFreq3"] = 500;
		dataRow["CFCEqFreq4"] = 1000;
		dataRow["CFCEqFreq5"] = 2000;
		dataRow["CFCEqFreq6"] = 3000;
		dataRow["CFCEqFreq7"] = 4000;
		dataRow["CFCEqFreq8"] = 5000;
		dataRow["CFCEqFreq9"] = 10000;
		dataRow["CFCParaEQData"] = "";
		dataTable.Rows.Add(dataRow);
		dataRow = dataTable.NewRow();
		dataRow["Name"] = "Default DX";
		dataRow["FilterLow"] = 200;
		dataRow["FilterHigh"] = 3100;
		dataRow["EQUseLegacy"] = true;
		dataRow["RXParaEQData"] = "";
		dataRow["TXParaEQData"] = "";
		dataRow["TXEQNumBands"] = 10;
		dataRow["TXEQEnabled"] = false;
		dataRow["TXEQPreamp"] = 0;
		dataRow["TXEQ1"] = 0;
		dataRow["TXEQ2"] = 0;
		dataRow["TXEQ3"] = 0;
		dataRow["TXEQ4"] = 0;
		dataRow["TXEQ5"] = 0;
		dataRow["TXEQ6"] = 0;
		dataRow["TXEQ7"] = 0;
		dataRow["TXEQ8"] = 0;
		dataRow["TXEQ9"] = 0;
		dataRow["TXEQ10"] = 0;
		dataRow["TxEqFreq1"] = 32;
		dataRow["TxEqFreq2"] = 63;
		dataRow["TxEqFreq3"] = 125;
		dataRow["TxEqFreq4"] = 250;
		dataRow["TxEqFreq5"] = 500;
		dataRow["TxEqFreq6"] = 1000;
		dataRow["TxEqFreq7"] = 2000;
		dataRow["TxEqFreq8"] = 4000;
		dataRow["TxEqFreq9"] = 8000;
		dataRow["TxEqFreq10"] = 16000;
		dataRow["DXOn"] = true;
		dataRow["DXLevel"] = 5;
		dataRow["CompanderOn"] = false;
		dataRow["CompanderLevel"] = 2;
		dataRow["MicGain"] = 5;
		dataRow["FMMicGain"] = 10;
		dataRow["MicMute"] = true;
		dataRow["Lev_On"] = true;
		dataRow["Lev_MaxGain"] = 15;
		dataRow["Lev_Decay"] = 100;
		dataRow["ALC_MaximumGain"] = 3;
		dataRow["ALC_Decay"] = 10;
		dataRow["Power"] = 50;
		dataRow["VOX_On"] = false;
		dataRow["Dexp_On"] = false;
		dataRow["Dexp_Threshold"] = -40;
		dataRow["Dexp_Attack"] = 2;
		dataRow["VOX_HangTime"] = 250;
		dataRow["Dexp_Release"] = 100;
		dataRow["Dexp_Attenuate"] = 10.0;
		dataRow["Dexp_Hysterisis"] = 2.0;
		dataRow["Dexp_Tau"] = 20;
		dataRow["Dexp_SCF_On"] = true;
		dataRow["Dexp_SCF_Low"] = 500;
		dataRow["Dexp_SCF_High"] = 1500;
		dataRow["Dexp_LookAhead_On"] = true;
		dataRow["Dexp_LookAhead"] = 60;
		dataRow["Tune_Power"] = 10;
		dataRow["Tune_Meter_Type"] = "Fwd Pwr";
		dataRow["TX_AF_Level"] = 50;
		dataRow["AM_Carrier_Level"] = 100;
		dataRow["Show_TX_Filter"] = true;
		dataRow["VAC1_On"] = false;
		dataRow["VAC1_Auto_On"] = false;
		dataRow["VAC1_RX_GAIN"] = 0;
		dataRow["VAC1_TX_GAIN"] = 0;
		dataRow["VAC1_Stereo_On"] = false;
		dataRow["VAC1_Sample_Rate"] = "48000";
		dataRow["VAC1_Buffer_Size"] = "2048";
		dataRow["VAC1_IQ_Output"] = false;
		dataRow["VAC1_IQ_Correct"] = true;
		dataRow["VAC1_PTT_OverRide"] = true;
		dataRow["VAC1_Combine_Input_Channels"] = false;
		dataRow["VAC1_Latency_On"] = true;
		dataRow["VAC1_Latency_Duration"] = 120;
		dataRow["VAC1_Latency_RBOut_On"] = true;
		dataRow["VAC1_Latency_RBOut_Duration"] = 120;
		dataRow["VAC1_Latency_PAIn_On"] = true;
		dataRow["VAC1_Latency_PAIn_Duration"] = 120;
		dataRow["VAC1_Latency_PAOut_On"] = true;
		dataRow["VAC1_Latency_PAOut_Duration"] = 120;
		dataRow["VAC1_AudioDriver"] = "";
		dataRow["VAC1_AudioInput"] = "";
		dataRow["VAC1_AudioOutput"] = "";
		dataRow["VAC2_On"] = false;
		dataRow["VAC2_Auto_On"] = false;
		dataRow["VAC2_RX_GAIN"] = 0;
		dataRow["VAC2_TX_GAIN"] = 0;
		dataRow["VAC2_Stereo_On"] = false;
		dataRow["VAC2_Sample_Rate"] = "48000";
		dataRow["VAC2_Buffer_Size"] = "2048";
		dataRow["VAC2_IQ_Output"] = false;
		dataRow["VAC2_IQ_Correct"] = true;
		dataRow["VAC2_Combine_Input_Channels"] = false;
		dataRow["VAC2_Latency_On"] = true;
		dataRow["VAC2_Latency_Duration"] = 120;
		dataRow["VAC2_Latency_RBOut_On"] = true;
		dataRow["VAC2_Latency_RBOut_Duration"] = 120;
		dataRow["VAC2_Latency_PAIn_On"] = true;
		dataRow["VAC2_Latency_PAIn_Duration"] = 120;
		dataRow["VAC2_Latency_PAOut_On"] = true;
		dataRow["VAC2_Latency_PAOut_Duration"] = 120;
		dataRow["VAC2_AudioDriver"] = "";
		dataRow["VAC2_AudioInput"] = "";
		dataRow["VAC2_AudioOutput"] = "";
		dataRow["Phone_RX_DSP_Buffer"] = "64";
		dataRow["Phone_TX_DSP_Buffer"] = "64";
		dataRow["FM_RX_DSP_Buffer"] = "256";
		dataRow["FM_TX_DSP_Buffer"] = "128";
		dataRow["Digi_RX_DSP_Buffer"] = "64";
		dataRow["Digi_TX_DSP_Buffer"] = "64";
		dataRow["CW_RX_DSP_Buffer"] = "64";
		dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
		dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
		dataRow["FM_RX_DSP_Filter_Size"] = "4096";
		dataRow["FM_TX_DSP_Filter_Size"] = "4096";
		dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
		dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
		dataRow["CW_RX_DSP_Filter_Size"] = "4096";
		dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
		dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
		dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
		dataRow["Mic_Input_On"] = true;
		dataRow["Mic_Input_Boost"] = true;
		dataRow["Line_Input_On"] = false;
		dataRow["Line_Input_Level"] = 0.0;
		dataRow["CESSB_On"] = false;
		dataRow["Pure_Signal_Enabled"] = false;
		dataRow["FM_RX_AFFilter_Low"] = 300;
		dataRow["FM_RX_AFFilter_High"] = 3000;
		dataRow["FM_TX_AFFilter_Low"] = 300;
		dataRow["FM_TX_AFFilter_High"] = 3000;
		dataRow["VAC1_Force_In"] = false;
		dataRow["VAC1_Force_Out"] = false;
		dataRow["VAC2_Force_In"] = false;
		dataRow["VAC2_Force_Out"] = false;
		dataRow["VAC1_SwapIQ"] = true;
		dataRow["VAC2_SwapIQ"] = true;
		dataRow["Audio_Disable_Audio_Amp"] = false;
		dataRow["PA_Profile"] = "";
		dataRow["RXEQEnabled"] = false;
		dataRow["RXEQPreamp"] = 0;
		dataRow["RXEQ1"] = 0;
		dataRow["RXEQ2"] = 0;
		dataRow["RXEQ3"] = 0;
		dataRow["RXEQ4"] = 0;
		dataRow["RXEQ5"] = 0;
		dataRow["RXEQ6"] = 0;
		dataRow["RXEQ7"] = 0;
		dataRow["RXEQ8"] = 0;
		dataRow["RXEQ9"] = 0;
		dataRow["RXEQ10"] = 0;
		dataRow["VAC1_Exclusive_In"] = false;
		dataRow["VAC1_Exclusive_Out"] = false;
		dataRow["VAC2_Exclusive_In"] = false;
		dataRow["VAC2_Exclusive_Out"] = false;
		dataRow["CFCUseLegacy"] = true;
		dataRow["CFCEnabled"] = false;
		dataRow["CFCPostEqEnabled"] = false;
		dataRow["CFCPhaseRotatorEnabled"] = false;
		dataRow["CFCPhaseReverseEnabled"] = false;
		dataRow["CFCPhaseRotatorFreq"] = 338;
		dataRow["CFCPhaseRotatorStages"] = 8;
		dataRow["CFCPhaseRotatorAuto"] = false;
		dataRow["CFCPreComp"] = 0;
		dataRow["CFCPostEqGain"] = 0;
		dataRow["CFCPreComp0"] = 5;
		dataRow["CFCPreComp1"] = 5;
		dataRow["CFCPreComp2"] = 5;
		dataRow["CFCPreComp3"] = 5;
		dataRow["CFCPreComp4"] = 5;
		dataRow["CFCPreComp5"] = 5;
		dataRow["CFCPreComp6"] = 5;
		dataRow["CFCPreComp7"] = 5;
		dataRow["CFCPreComp8"] = 5;
		dataRow["CFCPreComp9"] = 5;
		dataRow["CFCPostEqGain0"] = 0;
		dataRow["CFCPostEqGain1"] = 0;
		dataRow["CFCPostEqGain2"] = 0;
		dataRow["CFCPostEqGain3"] = 0;
		dataRow["CFCPostEqGain4"] = 0;
		dataRow["CFCPostEqGain5"] = 0;
		dataRow["CFCPostEqGain6"] = 0;
		dataRow["CFCPostEqGain7"] = 0;
		dataRow["CFCPostEqGain8"] = 0;
		dataRow["CFCPostEqGain9"] = 0;
		dataRow["CFCEqFreq0"] = 0;
		dataRow["CFCEqFreq1"] = 125;
		dataRow["CFCEqFreq2"] = 250;
		dataRow["CFCEqFreq3"] = 500;
		dataRow["CFCEqFreq4"] = 1000;
		dataRow["CFCEqFreq5"] = 2000;
		dataRow["CFCEqFreq6"] = 3000;
		dataRow["CFCEqFreq7"] = 4000;
		dataRow["CFCEqFreq8"] = 5000;
		dataRow["CFCEqFreq9"] = 10000;
		dataRow["CFCParaEQData"] = "";
		dataTable.Rows.Add(dataRow);
		if (bIndcludeExtraProfiles)
		{
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "Digi 1K@1500";
			dataRow["FilterLow"] = 1000;
			dataRow["FilterHigh"] = 2000;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 0;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 0;
			dataRow["MicGain"] = 5;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = false;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "Digi 1K@2210";
			dataRow["FilterLow"] = 1710;
			dataRow["FilterHigh"] = 2710;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 0;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 0;
			dataRow["MicGain"] = 5;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = false;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "AM";
			dataRow["FilterLow"] = 0;
			dataRow["FilterHigh"] = 4000;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "Conventional";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3100;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "D-104";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3500;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = -6;
			dataRow["TXEQ1"] = 7;
			dataRow["TXEQ2"] = 3;
			dataRow["TXEQ3"] = 4;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 5;
			dataRow["MicGain"] = 25;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "D-104+CPDR";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3500;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = -6;
			dataRow["TXEQ1"] = 7;
			dataRow["TXEQ2"] = 3;
			dataRow["TXEQ3"] = 4;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = true;
			dataRow["CompanderLevel"] = 5;
			dataRow["MicGain"] = 20;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "D-104+EQ";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3500;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = -6;
			dataRow["TXEQ1"] = 7;
			dataRow["TXEQ2"] = 3;
			dataRow["TXEQ3"] = 4;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 5;
			dataRow["MicGain"] = 20;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "DX / Contest";
			dataRow["FilterLow"] = 250;
			dataRow["FilterHigh"] = 3250;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = true;
			dataRow["DXLevel"] = 5;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "ESSB";
			dataRow["FilterLow"] = 50;
			dataRow["FilterHigh"] = 3650;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = true;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = false;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "HC4-5";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3100;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 5;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "HC4-5+CPDR";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3100;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = true;
			dataRow["CompanderLevel"] = 5;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "PR40+W2IHY";
			dataRow["FilterLow"] = 50;
			dataRow["FilterHigh"] = 3650;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "PR40+W2IHY+CPDR";
			dataRow["FilterLow"] = 50;
			dataRow["FilterHigh"] = 3650;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = false;
			dataRow["TXEQPreamp"] = 0;
			dataRow["TXEQ1"] = 0;
			dataRow["TXEQ2"] = 0;
			dataRow["TXEQ3"] = 0;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = true;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "PR781+EQ";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3200;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = -11;
			dataRow["TXEQ1"] = -6;
			dataRow["TXEQ2"] = 2;
			dataRow["TXEQ3"] = 8;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 3;
			dataRow["MicGain"] = 12;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "PR781+EQ+CPDR";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3200;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = -9;
			dataRow["TXEQ1"] = -8;
			dataRow["TXEQ2"] = 3;
			dataRow["TXEQ3"] = 7;
			dataRow["TXEQ4"] = 0;
			dataRow["TXEQ5"] = 0;
			dataRow["TXEQ6"] = 0;
			dataRow["TXEQ7"] = 0;
			dataRow["TXEQ8"] = 0;
			dataRow["TXEQ9"] = 0;
			dataRow["TXEQ10"] = 0;
			dataRow["TxEqFreq1"] = 32;
			dataRow["TxEqFreq2"] = 63;
			dataRow["TxEqFreq3"] = 125;
			dataRow["TxEqFreq4"] = 250;
			dataRow["TxEqFreq5"] = 500;
			dataRow["TxEqFreq6"] = 1000;
			dataRow["TxEqFreq7"] = 2000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 8000;
			dataRow["TxEqFreq10"] = 16000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = true;
			dataRow["CompanderLevel"] = 2;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 10;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "Fwd Pwr";
			dataRow["TX_AF_Level"] = 50;
			dataRow["AM_Carrier_Level"] = 100;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = false;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "2048";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = true;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 120;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 120;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 120;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 120;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 120;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 120;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 120;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 120;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = false;
			dataRow["CFCPostEqEnabled"] = false;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 0;
			dataRow["CFCPostEqGain"] = 0;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 5;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 5;
			dataRow["CFCPreComp7"] = 5;
			dataRow["CFCPreComp8"] = 5;
			dataRow["CFCPreComp9"] = 5;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = 0;
			dataRow["CFCPostEqGain2"] = 0;
			dataRow["CFCPostEqGain3"] = 0;
			dataRow["CFCPostEqGain4"] = 0;
			dataRow["CFCPostEqGain5"] = 0;
			dataRow["CFCPostEqGain6"] = 0;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 0;
			dataRow["CFCPostEqGain9"] = 0;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 125;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 2000;
			dataRow["CFCEqFreq6"] = 3000;
			dataRow["CFCEqFreq7"] = 4000;
			dataRow["CFCEqFreq8"] = 5000;
			dataRow["CFCEqFreq9"] = 10000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "SSB 2.8k CFC";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 2900;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = 4;
			dataRow["TXEQ1"] = -9;
			dataRow["TXEQ2"] = -6;
			dataRow["TXEQ3"] = -4;
			dataRow["TXEQ4"] = -3;
			dataRow["TXEQ5"] = -2;
			dataRow["TXEQ6"] = -1;
			dataRow["TXEQ7"] = -1;
			dataRow["TXEQ8"] = -1;
			dataRow["TXEQ9"] = -2;
			dataRow["TXEQ10"] = -2;
			dataRow["TxEqFreq1"] = 30;
			dataRow["TxEqFreq2"] = 70;
			dataRow["TxEqFreq3"] = 300;
			dataRow["TxEqFreq4"] = 500;
			dataRow["TxEqFreq5"] = 1000;
			dataRow["TxEqFreq6"] = 2000;
			dataRow["TxEqFreq7"] = 3000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 5000;
			dataRow["TxEqFreq10"] = 6000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 1;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 6;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "SWR";
			dataRow["TX_AF_Level"] = 100;
			dataRow["AM_Carrier_Level"] = 85;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = true;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "1024";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = false;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 60;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 60;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 60;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 60;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 60;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 60;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 60;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 60;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = true;
			dataRow["CFCPostEqEnabled"] = true;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 6;
			dataRow["CFCPostEqGain"] = -6;
			dataRow["CFCPreComp0"] = 5;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 4;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 6;
			dataRow["CFCPreComp6"] = 7;
			dataRow["CFCPreComp7"] = 8;
			dataRow["CFCPreComp8"] = 9;
			dataRow["CFCPreComp9"] = 9;
			dataRow["CFCPostEqGain0"] = -7;
			dataRow["CFCPostEqGain1"] = -7;
			dataRow["CFCPostEqGain2"] = -8;
			dataRow["CFCPostEqGain3"] = -8;
			dataRow["CFCPostEqGain4"] = -8;
			dataRow["CFCPostEqGain5"] = -7;
			dataRow["CFCPostEqGain6"] = -5;
			dataRow["CFCPostEqGain7"] = -4;
			dataRow["CFCPostEqGain8"] = -4;
			dataRow["CFCPostEqGain9"] = -5;
			dataRow["CFCEqFreq0"] = 100;
			dataRow["CFCEqFreq1"] = 150;
			dataRow["CFCEqFreq2"] = 300;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 750;
			dataRow["CFCEqFreq5"] = 1250;
			dataRow["CFCEqFreq6"] = 1750;
			dataRow["CFCEqFreq7"] = 2000;
			dataRow["CFCEqFreq8"] = 2600;
			dataRow["CFCEqFreq9"] = 2900;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "SSB 3.0k CFC";
			dataRow["FilterLow"] = 100;
			dataRow["FilterHigh"] = 3100;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = 4;
			dataRow["TXEQ1"] = -9;
			dataRow["TXEQ2"] = -6;
			dataRow["TXEQ3"] = -4;
			dataRow["TXEQ4"] = -3;
			dataRow["TXEQ5"] = -2;
			dataRow["TXEQ6"] = -1;
			dataRow["TXEQ7"] = -1;
			dataRow["TXEQ8"] = -1;
			dataRow["TXEQ9"] = -2;
			dataRow["TXEQ10"] = -2;
			dataRow["TxEqFreq1"] = 30;
			dataRow["TxEqFreq2"] = 70;
			dataRow["TxEqFreq3"] = 300;
			dataRow["TxEqFreq4"] = 500;
			dataRow["TxEqFreq5"] = 1000;
			dataRow["TxEqFreq6"] = 2000;
			dataRow["TxEqFreq7"] = 3000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 5000;
			dataRow["TxEqFreq10"] = 6000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 1;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 6;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "SWR";
			dataRow["TX_AF_Level"] = 100;
			dataRow["AM_Carrier_Level"] = 85;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = true;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "1024";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = false;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 60;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 60;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 60;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 60;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 60;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 60;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 60;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 60;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = true;
			dataRow["CFCPostEqEnabled"] = true;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 6;
			dataRow["CFCPostEqGain"] = -7;
			dataRow["CFCPreComp0"] = 6;
			dataRow["CFCPreComp1"] = 6;
			dataRow["CFCPreComp2"] = 5;
			dataRow["CFCPreComp3"] = 4;
			dataRow["CFCPreComp4"] = 5;
			dataRow["CFCPreComp5"] = 6;
			dataRow["CFCPreComp6"] = 7;
			dataRow["CFCPreComp7"] = 8;
			dataRow["CFCPreComp8"] = 9;
			dataRow["CFCPreComp9"] = 9;
			dataRow["CFCPostEqGain0"] = -4;
			dataRow["CFCPostEqGain1"] = -5;
			dataRow["CFCPostEqGain2"] = -7;
			dataRow["CFCPostEqGain3"] = -8;
			dataRow["CFCPostEqGain4"] = -8;
			dataRow["CFCPostEqGain5"] = -7;
			dataRow["CFCPostEqGain6"] = -4;
			dataRow["CFCPostEqGain7"] = -2;
			dataRow["CFCPostEqGain8"] = -1;
			dataRow["CFCPostEqGain9"] = -1;
			dataRow["CFCEqFreq0"] = 100;
			dataRow["CFCEqFreq1"] = 150;
			dataRow["CFCEqFreq2"] = 300;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 750;
			dataRow["CFCEqFreq5"] = 1250;
			dataRow["CFCEqFreq6"] = 1750;
			dataRow["CFCEqFreq7"] = 2000;
			dataRow["CFCEqFreq8"] = 2600;
			dataRow["CFCEqFreq9"] = 3100;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "SSB 3.3k CFC";
			dataRow["FilterLow"] = 50;
			dataRow["FilterHigh"] = 3350;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = 4;
			dataRow["TXEQ1"] = -9;
			dataRow["TXEQ2"] = -6;
			dataRow["TXEQ3"] = -4;
			dataRow["TXEQ4"] = -3;
			dataRow["TXEQ5"] = -2;
			dataRow["TXEQ6"] = -1;
			dataRow["TXEQ7"] = -1;
			dataRow["TXEQ8"] = -1;
			dataRow["TXEQ9"] = -2;
			dataRow["TXEQ10"] = -2;
			dataRow["TxEqFreq1"] = 30;
			dataRow["TxEqFreq2"] = 70;
			dataRow["TxEqFreq3"] = 300;
			dataRow["TxEqFreq4"] = 500;
			dataRow["TxEqFreq5"] = 1000;
			dataRow["TxEqFreq6"] = 2000;
			dataRow["TxEqFreq7"] = 3000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 5000;
			dataRow["TxEqFreq10"] = 6000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 1;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 6;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "SWR";
			dataRow["TX_AF_Level"] = 100;
			dataRow["AM_Carrier_Level"] = 85;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = true;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "1024";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = false;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 60;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 60;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 60;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 60;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 60;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 60;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 60;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 60;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = true;
			dataRow["CFCPostEqEnabled"] = true;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 8;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 7;
			dataRow["CFCPostEqGain"] = -6;
			dataRow["CFCPreComp0"] = 4;
			dataRow["CFCPreComp1"] = 4;
			dataRow["CFCPreComp2"] = 3;
			dataRow["CFCPreComp3"] = 3;
			dataRow["CFCPreComp4"] = 4;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 6;
			dataRow["CFCPreComp7"] = 7;
			dataRow["CFCPreComp8"] = 8;
			dataRow["CFCPreComp9"] = 8;
			dataRow["CFCPostEqGain0"] = -6;
			dataRow["CFCPostEqGain1"] = -6;
			dataRow["CFCPostEqGain2"] = -7;
			dataRow["CFCPostEqGain3"] = -8;
			dataRow["CFCPostEqGain4"] = -8;
			dataRow["CFCPostEqGain5"] = -7;
			dataRow["CFCPostEqGain6"] = -5;
			dataRow["CFCPostEqGain7"] = -5;
			dataRow["CFCPostEqGain8"] = -4;
			dataRow["CFCPostEqGain9"] = -5;
			dataRow["CFCEqFreq0"] = 50;
			dataRow["CFCEqFreq1"] = 150;
			dataRow["CFCEqFreq2"] = 300;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 750;
			dataRow["CFCEqFreq5"] = 1250;
			dataRow["CFCEqFreq6"] = 1750;
			dataRow["CFCEqFreq7"] = 2000;
			dataRow["CFCEqFreq8"] = 2600;
			dataRow["CFCEqFreq9"] = 3350;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
			dataRow = dataTable.NewRow();
			dataRow["Name"] = "AM 10k CFC";
			dataRow["FilterLow"] = 0;
			dataRow["FilterHigh"] = 5000;
			dataRow["EQUseLegacy"] = true;
			dataRow["RXParaEQData"] = "";
			dataRow["TXParaEQData"] = "";
			dataRow["TXEQNumBands"] = 10;
			dataRow["TXEQEnabled"] = true;
			dataRow["TXEQPreamp"] = 4;
			dataRow["TXEQ1"] = -9;
			dataRow["TXEQ2"] = -6;
			dataRow["TXEQ3"] = -4;
			dataRow["TXEQ4"] = -3;
			dataRow["TXEQ5"] = -2;
			dataRow["TXEQ6"] = -1;
			dataRow["TXEQ7"] = -1;
			dataRow["TXEQ8"] = -1;
			dataRow["TXEQ9"] = -2;
			dataRow["TXEQ10"] = -2;
			dataRow["TxEqFreq1"] = 30;
			dataRow["TxEqFreq2"] = 70;
			dataRow["TxEqFreq3"] = 300;
			dataRow["TxEqFreq4"] = 500;
			dataRow["TxEqFreq5"] = 1000;
			dataRow["TxEqFreq6"] = 2000;
			dataRow["TxEqFreq7"] = 3000;
			dataRow["TxEqFreq8"] = 4000;
			dataRow["TxEqFreq9"] = 5000;
			dataRow["TxEqFreq10"] = 6000;
			dataRow["DXOn"] = false;
			dataRow["DXLevel"] = 3;
			dataRow["CompanderOn"] = false;
			dataRow["CompanderLevel"] = 1;
			dataRow["MicGain"] = 10;
			dataRow["FMMicGain"] = 6;
			dataRow["MicMute"] = true;
			dataRow["Lev_On"] = true;
			dataRow["Lev_MaxGain"] = 15;
			dataRow["Lev_Decay"] = 100;
			dataRow["ALC_MaximumGain"] = 3;
			dataRow["ALC_Decay"] = 10;
			dataRow["Power"] = 50;
			dataRow["VOX_On"] = false;
			dataRow["Dexp_On"] = false;
			dataRow["Dexp_Threshold"] = -40;
			dataRow["Dexp_Attack"] = 2;
			dataRow["VOX_HangTime"] = 250;
			dataRow["Dexp_Release"] = 100;
			dataRow["Dexp_Attenuate"] = 10.0;
			dataRow["Dexp_Hysterisis"] = 2.0;
			dataRow["Dexp_Tau"] = 20;
			dataRow["Dexp_SCF_On"] = true;
			dataRow["Dexp_SCF_Low"] = 500;
			dataRow["Dexp_SCF_High"] = 1500;
			dataRow["Dexp_LookAhead_On"] = true;
			dataRow["Dexp_LookAhead"] = 60;
			dataRow["Tune_Power"] = 10;
			dataRow["Tune_Meter_Type"] = "SWR";
			dataRow["TX_AF_Level"] = 100;
			dataRow["AM_Carrier_Level"] = 95;
			dataRow["Show_TX_Filter"] = true;
			dataRow["VAC1_On"] = false;
			dataRow["VAC1_Auto_On"] = false;
			dataRow["VAC1_RX_GAIN"] = 0;
			dataRow["VAC1_TX_GAIN"] = 0;
			dataRow["VAC1_Stereo_On"] = true;
			dataRow["VAC1_Sample_Rate"] = "48000";
			dataRow["VAC1_Buffer_Size"] = "1024";
			dataRow["VAC1_IQ_Output"] = false;
			dataRow["VAC1_IQ_Correct"] = true;
			dataRow["VAC1_PTT_OverRide"] = false;
			dataRow["VAC1_Combine_Input_Channels"] = false;
			dataRow["VAC1_Latency_On"] = true;
			dataRow["VAC1_Latency_Duration"] = 60;
			dataRow["VAC1_Latency_RBOut_On"] = true;
			dataRow["VAC1_Latency_RBOut_Duration"] = 60;
			dataRow["VAC1_Latency_PAIn_On"] = true;
			dataRow["VAC1_Latency_PAIn_Duration"] = 60;
			dataRow["VAC1_Latency_PAOut_On"] = true;
			dataRow["VAC1_Latency_PAOut_Duration"] = 60;
			dataRow["VAC1_AudioDriver"] = "";
			dataRow["VAC1_AudioInput"] = "";
			dataRow["VAC1_AudioOutput"] = "";
			dataRow["VAC2_On"] = false;
			dataRow["VAC2_Auto_On"] = false;
			dataRow["VAC2_RX_GAIN"] = 0;
			dataRow["VAC2_TX_GAIN"] = 0;
			dataRow["VAC2_Stereo_On"] = false;
			dataRow["VAC2_Sample_Rate"] = "48000";
			dataRow["VAC2_Buffer_Size"] = "2048";
			dataRow["VAC2_IQ_Output"] = false;
			dataRow["VAC2_IQ_Correct"] = true;
			dataRow["VAC2_Combine_Input_Channels"] = false;
			dataRow["VAC2_Latency_On"] = true;
			dataRow["VAC2_Latency_Duration"] = 60;
			dataRow["VAC2_Latency_RBOut_On"] = true;
			dataRow["VAC2_Latency_RBOut_Duration"] = 60;
			dataRow["VAC2_Latency_PAIn_On"] = true;
			dataRow["VAC2_Latency_PAIn_Duration"] = 60;
			dataRow["VAC2_Latency_PAOut_On"] = true;
			dataRow["VAC2_Latency_PAOut_Duration"] = 60;
			dataRow["VAC2_AudioDriver"] = "";
			dataRow["VAC2_AudioInput"] = "";
			dataRow["VAC2_AudioOutput"] = "";
			dataRow["Phone_RX_DSP_Buffer"] = "64";
			dataRow["Phone_TX_DSP_Buffer"] = "64";
			dataRow["FM_RX_DSP_Buffer"] = "256";
			dataRow["FM_TX_DSP_Buffer"] = "128";
			dataRow["Digi_RX_DSP_Buffer"] = "64";
			dataRow["Digi_TX_DSP_Buffer"] = "64";
			dataRow["CW_RX_DSP_Buffer"] = "64";
			dataRow["Phone_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_TX_DSP_Filter_Size"] = "4096";
			dataRow["FM_RX_DSP_Filter_Size"] = "4096";
			dataRow["FM_TX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_RX_DSP_Filter_Size"] = "4096";
			dataRow["Digi_TX_DSP_Filter_Size"] = "4096";
			dataRow["CW_RX_DSP_Filter_Size"] = "4096";
			dataRow["Phone_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Phone_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["FM_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Digi_TX_DSP_Filter_Type"] = "Low Latency";
			dataRow["CW_RX_DSP_Filter_Type"] = "Low Latency";
			dataRow["Mic_Input_On"] = true;
			dataRow["Mic_Input_Boost"] = true;
			dataRow["Line_Input_On"] = false;
			dataRow["Line_Input_Level"] = 0.0;
			dataRow["CESSB_On"] = false;
			dataRow["Pure_Signal_Enabled"] = false;
			dataRow["FM_RX_AFFilter_Low"] = 300;
			dataRow["FM_RX_AFFilter_High"] = 3000;
			dataRow["FM_TX_AFFilter_Low"] = 300;
			dataRow["FM_TX_AFFilter_High"] = 3000;
			dataRow["VAC1_Force_In"] = false;
			dataRow["VAC1_Force_Out"] = false;
			dataRow["VAC2_Force_In"] = false;
			dataRow["VAC2_Force_Out"] = false;
			dataRow["VAC1_SwapIQ"] = true;
			dataRow["VAC2_SwapIQ"] = true;
			dataRow["Audio_Disable_Audio_Amp"] = false;
			dataRow["PA_Profile"] = "";
			dataRow["RXEQEnabled"] = false;
			dataRow["RXEQPreamp"] = 0;
			dataRow["RXEQ1"] = 0;
			dataRow["RXEQ2"] = 0;
			dataRow["RXEQ3"] = 0;
			dataRow["RXEQ4"] = 0;
			dataRow["RXEQ5"] = 0;
			dataRow["RXEQ6"] = 0;
			dataRow["RXEQ7"] = 0;
			dataRow["RXEQ8"] = 0;
			dataRow["RXEQ9"] = 0;
			dataRow["RXEQ10"] = 0;
			dataRow["VAC1_Exclusive_In"] = false;
			dataRow["VAC1_Exclusive_Out"] = false;
			dataRow["VAC2_Exclusive_In"] = false;
			dataRow["VAC2_Exclusive_Out"] = false;
			dataRow["CFCUseLegacy"] = true;
			dataRow["CFCEnabled"] = true;
			dataRow["CFCPostEqEnabled"] = true;
			dataRow["CFCPhaseRotatorEnabled"] = false;
			dataRow["CFCPhaseReverseEnabled"] = false;
			dataRow["CFCPhaseRotatorFreq"] = 338;
			dataRow["CFCPhaseRotatorStages"] = 9;
			dataRow["CFCPhaseRotatorAuto"] = false;
			dataRow["CFCPreComp"] = 6;
			dataRow["CFCPostEqGain"] = -8;
			dataRow["CFCPreComp0"] = 6;
			dataRow["CFCPreComp1"] = 5;
			dataRow["CFCPreComp2"] = 4;
			dataRow["CFCPreComp3"] = 4;
			dataRow["CFCPreComp4"] = 4;
			dataRow["CFCPreComp5"] = 5;
			dataRow["CFCPreComp6"] = 6;
			dataRow["CFCPreComp7"] = 7;
			dataRow["CFCPreComp8"] = 7;
			dataRow["CFCPreComp9"] = 7;
			dataRow["CFCPostEqGain0"] = 0;
			dataRow["CFCPostEqGain1"] = -1;
			dataRow["CFCPostEqGain2"] = -2;
			dataRow["CFCPostEqGain3"] = -3;
			dataRow["CFCPostEqGain4"] = -3;
			dataRow["CFCPostEqGain5"] = -2;
			dataRow["CFCPostEqGain6"] = -1;
			dataRow["CFCPostEqGain7"] = 0;
			dataRow["CFCPostEqGain8"] = 1;
			dataRow["CFCPostEqGain9"] = 1;
			dataRow["CFCEqFreq0"] = 0;
			dataRow["CFCEqFreq1"] = 70;
			dataRow["CFCEqFreq2"] = 250;
			dataRow["CFCEqFreq3"] = 500;
			dataRow["CFCEqFreq4"] = 1000;
			dataRow["CFCEqFreq5"] = 1500;
			dataRow["CFCEqFreq6"] = 2000;
			dataRow["CFCEqFreq7"] = 3000;
			dataRow["CFCEqFreq8"] = 4000;
			dataRow["CFCEqFreq9"] = 5000;
			dataRow["CFCParaEQData"] = "";
			dataTable.Rows.Add(dataRow);
		}
	}

	private static void CheckBandTextValid()
	{
		List<DataRow> list = new List<DataRow>();
		if (ds == null)
		{
			return;
		}
		foreach (DataRow row in ds.Tables["BandText"].Rows)
		{
			string text = ((double)row["Low"]).ToString("f6");
			text = text.Replace(",", ".");
			if (ds.Tables["BandText"].Select(text + ">=Low AND " + text + "<=High").Length > 1 && !list.Contains(row))
			{
				list.Add(row);
			}
			text = ((double)row["High"]).ToString("f6");
			text = text.Replace(",", ".");
			if (ds.Tables["BandText"].Select(text + ">=Low AND " + text + "<=High").Length > 1 && !list.Contains(row))
			{
				list.Add(row);
			}
		}
		foreach (DataRow item in list)
		{
			ds.Tables["BandText"].Rows.Remove(item);
		}
	}

	public static bool Init()
	{
		_merged = false;
		ds = new DataSet("Data");
		if (File.Exists(_file_name))
		{
			try
			{
				ds.ReadXml(_file_name);
			}
			catch
			{
				return false;
			}
		}
		VerifyTables();
		CheckBandTextValid();
		bool flag = false;
		Dictionary<string, string> dict = GetVarsDictionary("State");
		if (!dict.ContainsKey("Version"))
		{
			VersionString = TitleBar.GetString(bWithFirmware: false);
			dict.Add("Version", VersionString);
			flag = true;
		}
		else
		{
			VersionString = ConvertFromDBVal<string>(dict["Version"]);
		}
		if (!dict.ContainsKey("VersionNumber"))
		{
			VersionNumber = Common.GetVerNum();
			dict.Add("VersionNumber", VersionNumber);
			flag = true;
		}
		else
		{
			VersionNumber = ConvertFromDBVal<string>(dict["VersionNumber"]);
		}
		if (flag)
		{
			SaveVarsDictionary("State", ref dict);
		}
		return true;
	}

	public static T ConvertFromDBVal<T>(object obj)
	{
		if (obj == null || obj == DBNull.Value)
		{
			return default(T);
		}
		return (T)obj;
	}

	public static void WriteDB()
	{
		WriteDB(_file_name, ds);
	}

	public static bool WriteDB(string fn)
	{
		return WriteDB(fn, ds);
	}

	public static bool WriteDB(string fn, DataSet dsIN)
	{
		try
		{
			dsIN.WriteXml(fn, XmlWriteMode.WriteSchema);
			DBMan.DBWritten();
		}
		catch (Exception ex)
		{
			MessageBox.Show("A database write to file operation failed.  The exception error was:\n\n" + ex.Message, "ERROR: Database Write Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return false;
		}
		return true;
	}

	public static void Exit()
	{
		WriteDB();
		ds = null;
	}

	public static bool BandText(double freq, out string outStr)
	{
		try
		{
			outStr = "";
			if (ds == null)
			{
				return false;
			}
			string text = freq.ToString("f6");
			text = text.Replace(",", ".");
			DataRow[] array = ds.Tables["BandText"].Select(text + ">=Low AND " + text + "<=High");
			if (array.Length == 0)
			{
				outStr = "Out of Band";
				return false;
			}
			if (array.Length == 1)
			{
				outStr = (string)array[0]["Name"];
				return (bool)array[0]["TX"];
			}
			MessageBox.Show("Error reading BandInfo table.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			outStr = "Error";
			return false;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message + "\n\n\n" + ex.StackTrace, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			outStr = "Error";
			return false;
		}
	}

	public static void PurgeNotches()
	{
		purgeTableEntries("State", "mnotchdb*");
	}

	public static void PurgeMeters(List<string> formGuids)
	{
		if (ds == null)
		{
			return;
		}
		purgeTableEntries("Options", "meterContData_*");
		purgeTableEntries("Options", "meterData_*");
		purgeTableEntries("Options", "meterIGData_*");
		purgeTableEntries("Options", "meterIGSettings_*");
		if (formGuids == null)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (DataTable table in ds.Tables)
		{
			string tableName = table.TableName;
			if (tableName.StartsWith("MeterDisplay_"))
			{
				string item = tableName.Substring(tableName.IndexOf("_") + 1);
				if (!formGuids.Contains(item))
				{
					list.Add(tableName);
				}
			}
		}
		foreach (string item2 in list)
		{
			if (ds.Tables.Contains(item2))
			{
				ds.Tables.Remove(item2);
			}
		}
	}

	private static void purgeTableEntries(string sTable, string sKey)
	{
		if (ds == null || !ds.Tables.Contains(sTable))
		{
			return;
		}
		DataRow[] array = ds.Tables[sTable].Select("Key like '" + sKey + "'");
		if (array != null)
		{
			DataRow[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Delete();
			}
		}
	}

	public static void RemoveVarsList(string tableName, List<string> list)
	{
		if (ds == null)
		{
			return;
		}
		if (!ds.Tables.Contains(tableName))
		{
			AddFormTable(tableName);
		}
		checkForPrimaryKeys(tableName);
		foreach (string item in list)
		{
			DataRow dataRow = null;
			try
			{
				dataRow = ds.Tables[tableName].Rows.Find(item);
			}
			catch
			{
			}
			dataRow?.Delete();
		}
	}

	public static void SaveVarsDictionary(string tableName, ref Dictionary<string, string> dict, bool bSaveEmptyValues = true)
	{
		if (_merged || ds == null)
		{
			return;
		}
		if (!ds.Tables.Contains(tableName))
		{
			AddFormTable(tableName);
		}
		checkForPrimaryKeys(tableName);
		foreach (KeyValuePair<string, string> item in dict)
		{
			string key = item.Key;
			string value = item.Value;
			if (bSaveEmptyValues || value.Length != 0)
			{
				DataRow dataRow = null;
				try
				{
					dataRow = ds.Tables[tableName].Rows.Find(key);
				}
				catch
				{
				}
				if (dataRow != null)
				{
					dataRow[1] = value;
					continue;
				}
				DataRow dataRow2 = ds.Tables[tableName].NewRow();
				dataRow2[0] = key;
				dataRow2[1] = value;
				ds.Tables[tableName].Rows.Add(dataRow2);
			}
		}
	}

	public static void SaveVars(string tableName, List<string> list, bool bSaveEmptyValues = true)
	{
		if (_merged || ds == null)
		{
			return;
		}
		if (!ds.Tables.Contains(tableName))
		{
			AddFormTable(tableName);
		}
		checkForPrimaryKeys(tableName);
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			string[] array = item.Split('/');
			if (array.Length > 2)
			{
				for (int i = 2; i < array.Length; i++)
				{
					ref string reference = ref array[1];
					reference = reference + "/" + array[i];
				}
			}
			if (bSaveEmptyValues || array.Length > 1)
			{
				DataRow dataRow = null;
				try
				{
					dataRow = ds.Tables[tableName].Rows.Find(array[0]);
				}
				catch
				{
				}
				if (dataRow != null)
				{
					dataRow[1] = array[1];
					continue;
				}
				DataRow dataRow2 = ds.Tables[tableName].NewRow();
				dataRow2[0] = array[0];
				dataRow2[1] = array[1];
				ds.Tables[tableName].Rows.Add(dataRow2);
			}
		}
	}

	public static Dictionary<string, string> GetVarsDictionary(string table_name)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		if (ds == null)
		{
			return result;
		}
		if (!ds.Tables.Contains(table_name))
		{
			return result;
		}
		DataTable dataTable = ds.Tables[table_name];
		result = new Dictionary<string, string>(dataTable.Rows.Count);
		foreach (DataRow row in dataTable.Rows)
		{
			string key = row[0].ToString();
			string value = row[1].ToString();
			result.Add(key, value);
		}
		return result;
	}

	public static List<string> GetVars(string tableName)
	{
		List<string> list = new List<string>();
		if (ds == null)
		{
			return list;
		}
		if (!ds.Tables.Contains(tableName))
		{
			return list;
		}
		DataTable dataTable = ds.Tables[tableName];
		for (int i = 0; i < dataTable.Rows.Count; i++)
		{
			list.Add(dataTable.Rows[i][0].ToString() + "/" + dataTable.Rows[i][1].ToString());
		}
		return list;
	}

	public static bool ImportAndMergeDatabase(string filename, out string log, bool ignore_merged)
	{
		_merged = false;
		string version = "";
		log = "";
		if (!File.Exists(filename))
		{
			return false;
		}
		DataSet dataSet = ds.Copy();
		DataSet dataSet2 = new DataSet();
		try
		{
			dataSet2.ReadXml(filename);
		}
		catch (Exception)
		{
			return false;
		}
		log = log + "Read <" + filename + ">\n\n";
		string text = ValidateImportedDatabase(dataSet2);
		if (text != "")
		{
			log += text;
			return false;
		}
		DataSet dataSet3 = ds.Clone();
		List<string> list = new List<string>();
		bool flag = false;
		bool flag2 = false;
		if (dataSet2.Tables.Contains("Options"))
		{
			DataRow[] array = dataSet2.Tables["Options"].Select("Key like 'meterContData_*'");
			flag = array != null && array.Length != 0;
			array = dataSet2.Tables["Options"].Select("Key like 'PAProfile*'");
			flag2 = array != null && array.Length != 0;
		}
		if (dataSet2.Tables.Contains("State"))
		{
			foreach (DataRow row3 in dataSet2.Tables["State"].Rows)
			{
				if (Convert.ToString(row3["Key"]) == "VersionNumber")
				{
					version = Convert.ToString(row3["Value"]);
					break;
				}
			}
		}
		foreach (DataTable table in dataSet2.Tables)
		{
			if (!dataSet.Tables.Contains(table.TableName))
			{
				dataSet3.Merge(table);
			}
		}
		foreach (DataTable table2 in dataSet.Tables)
		{
			_ = dataSet2.Tables;
			bool flag3 = false;
			DataTable dataTable3 = table2.Clone();
			DataTable tempTable = table2.Clone();
			string tableName = table2.TableName;
			switch (tableName)
			{
			default:
				if (tableName == null || !tableName.StartsWith("MeterDisplay_"))
				{
					break;
				}
				goto case "DBManForm";
			case "BandStack2Entries":
			case "BandStack2Filters":
			case "BandStack2FilterBands":
			case "BandStack2FilterModes":
			case "BandStack2FilterFrequencies":
			case "BandStack2FilterSubModes":
				dataTable3.Clear();
				foreach (DataTable table3 in dataSet2.Tables)
				{
					if (table3.TableName == table2.TableName)
					{
						tempTable = table3.Copy();
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					dataSet3.Merge(table2);
					log = log + "New table not found in imported database: " + table2.TableName + "\n";
					continue;
				}
				foreach (DataRow row4 in tempTable.Rows)
				{
					dataTable3.ImportRow(row4);
				}
				if (Common.CompareVersions(version, "2.8.12") < 0 && table2.TableName == "BandStack2Entries")
				{
					flag3 = false;
					foreach (DataTable table4 in dataSet2.Tables)
					{
						if (table4.TableName == "BandStack")
						{
							tempTable = table4.Copy();
							flag3 = true;
							break;
						}
					}
					if (flag3)
					{
						foreach (DataRow row5 in tempTable.Rows)
						{
							string filterExpression9 = "Frequency = " + (double)row5["Freq"];
							if (dataTable3.Select(filterExpression9).Length != 0)
							{
								continue;
							}
							try
							{
								DataRow dataRow12 = dataTable3.NewRow();
								dataRow12["GUID"] = Guid.NewGuid().ToString();
								dataRow12["Description"] = ConvertFromDBVal<string>(row5["BandName"]);
								dataRow12["Frequency"] = Math.Round((double)row5["Freq"], 6);
								dataRow12["CentreFrequency"] = Math.Round((double)row5["CenterFreq"], 6);
								dataRow12["Band"] = (int)BandStackManager.StringToBand(ConvertFromDBVal<string>(row5["BandName"]));
								dataRow12["Mode"] = (int)BandStackManager.StringToMode(ConvertFromDBVal<string>(row5["Mode"]));
								dataRow12["SubMode"] = -1;
								dataRow12["CTUNEnabled"] = (bool)row5["CTUN"];
								string text13 = ConvertFromDBVal<string>(row5["Filter"]);
								if (text13.EndsWith("@"))
								{
									dataRow12["Locked"] = true;
									text13 = text13.Substring(0, text13.Length - 1);
								}
								else
								{
									dataRow12["Locked"] = false;
								}
								dataRow12["Filter"] = (int)BandStackManager.StringToFilter(text13);
								dataRow12["ZoomFactor"] = (double)row5["ZoomFactor"];
								dataRow12["ZoomSlider"] = (int)(double)row5["ZoomFactor"];
								dataTable3.Rows.Add(dataRow12);
							}
							catch
							{
							}
						}
					}
				}
				dataSet3.Merge(dataTable3);
				log = log + "Imported table <" + table2.TableName + "> into database.\n";
				continue;
			case "GroupList":
			case "Memory":
			case "BandText":
				dataSet3.Merge(table2);
				log = log + "Did not import table <" + table2.TableName + "> into database.\n";
				continue;
			case "TXProfile":
			{
				DataTable oldTable = dataSet2.Tables["TXProfile"].Clone();
				dataTable3.Clear();
				foreach (DataTable table5 in dataSet2.Tables)
				{
					if (table5.TableName == table2.TableName)
					{
						oldTable = table5.Copy();
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					continue;
				}
				DataTable dataTable5 = ExpandOldTxProfileTable(oldTable);
				if (dataTable5 != null)
				{
					dataTable3.Merge(dataTable5);
					foreach (DataRow row6 in table2.Rows)
					{
						string filterExpression = "Name = '" + row6["Name"]?.ToString() + "'";
						if (dataTable3.Select(filterExpression).Length == 0)
						{
							dataTable3.ImportRow(row6);
						}
					}
					dataSet3.Merge(dataTable3);
					log = log + "Imported table <" + table2.TableName + "> into database.\n";
				}
				else
				{
					dataTable3.Merge(table2);
				}
				continue;
			}
			case "TXProfileDef":
				dataSet3.Merge(table2);
				log = log + "Did not import table <" + table2.TableName + "> into database.\n";
				continue;
			case "DBManForm":
			case "EQForm":
			case "AmpView":
			case "Options":
			case "FilterForm":
			case "MemoryForm":
			case "PureSignal":
			case "State":
			case "DiversityForm":
			case "BandStack2Form":
				tempTable.Clear();
				dataTable3.Clear();
				foreach (DataTable table6 in dataSet2.Tables)
				{
					if (table6.TableName == table2.TableName)
					{
						tempTable = table6.Copy();
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					dataSet3.Merge(table2);
					log = log + "New table not found in imported database: " + table2.TableName + "\n";
					continue;
				}
				foreach (DataRow row7 in table2.Rows)
				{
					string text2 = Convert.ToString(row7["Key"]);
					if (text2 == "VersionNumber")
					{
						row7["Value"] = VersionNumber;
						dataTable3.ImportRow(row7);
					}
					else if (text2 == "Version")
					{
						row7["Value"] = VersionString;
						dataTable3.ImportRow(row7);
					}
					else if (text2.Contains("_by_band"))
					{
						DataRow dataRow4 = row7;
						string filterExpression2 = "Key = '" + text2 + "'";
						DataRow[] array2 = tempTable.Select(filterExpression2);
						if (array2.Length != 0)
						{
							string text3 = Convert.ToString(array2[0]["Value"]);
							string text4 = text3;
							int length = Convert.ToString(dataRow4["Value"]).Length;
							string text5 = "|" + text3[text3.Length - 1];
							for (int i = text3.Length; i < length; i += 2)
							{
								text4 += text5;
							}
							dataRow4["Value"] = text4;
							dataTable3.ImportRow(dataRow4);
						}
						else
						{
							dataTable3.ImportRow(row7);
						}
					}
					else if (text2 == "comboRadioModel" && Common.CompareVersions(version, "2.7.0") < 0)
					{
						string text6 = "";
						if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelHPSDR"))
						{
							text6 = "HPSDR";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelHermes"))
						{
							text6 = "HERMES";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN10"))
						{
							text6 = "ANAN-10";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN10E"))
						{
							text6 = "ANAN-10E";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN100"))
						{
							text6 = "ANAN-100";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN100B"))
						{
							text6 = "ANAN-100B";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN100D"))
						{
							text6 = "ANAN-100D";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN200D"))
						{
							text6 = "ANAN-200D";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D"))
						{
							text6 = "ANAN-7000DLE";
						}
						else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN8000D"))
						{
							text6 = "ANAN-8000DLE";
						}
						if (text6 != "")
						{
							row7["Value"] = text6;
							dataTable3.ImportRow(row7);
						}
						else
						{
							dataTable3.ImportRow(row7);
						}
					}
					else if (text2.Contains("mnotchdb"))
					{
						list.Add(row7["Value"].ToString());
					}
					else if (tempTable.TableName == "Options" && (text2.StartsWith("meterContData_") || text2.StartsWith("meterData_") || text2.StartsWith("meterIGData_") || text2.StartsWith("meterIGSettings_")))
					{
						dataTable3.ImportRow(row7);
					}
					else if (!flag2 || !(tempTable.TableName == "Options") || !text2.StartsWith("PAProfile"))
					{
						string filterExpression3 = "Key = '" + text2 + "'";
						DataRow[] array3 = tempTable.Select(filterExpression3);
						if (array3.Length != 0)
						{
							dataTable3.ImportRow(array3[0]);
						}
						else
						{
							dataTable3.ImportRow(row7);
						}
					}
				}
				if (tempTable.TableName == "Options")
				{
					if (flag)
					{
						DataRow[] array4 = tempTable.Select("Key like 'meterContData_*'");
						if (array4 != null)
						{
							DataRow[] array5 = array4;
							foreach (DataRow dataRow5 in array5)
							{
								string text7 = dataRow5["Key"].ToString();
								string filterExpression4 = "Key = '" + text7 + "'";
								if (dataTable3.Select(filterExpression4).Length == 0)
								{
									dataTable3.ImportRow(dataRow5);
								}
							}
						}
						array4 = tempTable.Select("Key like 'meterData_*'");
						if (array4 != null)
						{
							DataRow[] array5 = array4;
							foreach (DataRow dataRow6 in array5)
							{
								string text8 = dataRow6["Key"].ToString();
								string filterExpression5 = "Key = '" + text8 + "'";
								if (dataTable3.Select(filterExpression5).Length == 0)
								{
									dataTable3.ImportRow(dataRow6);
								}
							}
						}
						array4 = tempTable.Select("Key like 'meterIGData_*'");
						if (array4 != null)
						{
							DataRow[] array5 = array4;
							foreach (DataRow dataRow7 in array5)
							{
								string text9 = dataRow7["Key"].ToString();
								string filterExpression6 = "Key = '" + text9 + "'";
								if (dataTable3.Select(filterExpression6).Length == 0)
								{
									dataTable3.ImportRow(dataRow7);
								}
							}
						}
						array4 = tempTable.Select("Key like 'meterIGSettings_*'");
						if (array4 != null)
						{
							DataRow[] array5 = array4;
							foreach (DataRow dataRow8 in array5)
							{
								string text10 = dataRow8["Key"].ToString();
								string filterExpression7 = "Key = '" + text10 + "'";
								if (dataTable3.Select(filterExpression7).Length == 0)
								{
									dataTable3.ImportRow(dataRow8);
								}
							}
						}
					}
					if (flag2)
					{
						DataRow[] array6 = tempTable.Select("Key like 'PAProfile*'");
						if (array6 != null)
						{
							DataRow[] array5 = array6;
							foreach (DataRow row in array5)
							{
								dataTable3.ImportRow(row);
							}
						}
					}
				}
				if (tempTable.TableName == "State" && dataSet2.Tables.Contains("State"))
				{
					DataRow[] array7 = dataSet2.Tables["State"].Select("Key like 'mnotchdb*'");
					if (array7 != null)
					{
						DataRow[] array5 = array7;
						foreach (DataRow dataRow9 in array5)
						{
							if (!Convert.ToString(dataRow9["Key"]).Contains("mnotchdb"))
							{
								continue;
							}
							string text11 = dataRow9["Value"].ToString();
							MNotch mNotch = MNotch.Parse(text11);
							bool flag4 = false;
							foreach (string item in list)
							{
								if (MNotch.Parse(item).FCenter == mNotch.FCenter)
								{
									flag4 = true;
									break;
								}
							}
							if (!flag4)
							{
								list.Add(text11);
							}
						}
					}
					int num = 0;
					foreach (string item2 in list)
					{
						DataRow dataRow10 = dataTable3.NewRow();
						dataRow10["Key"] = "mnotchdb[" + num + "]";
						dataRow10["Value"] = item2;
						dataTable3.Rows.Add(dataRow10);
						num++;
					}
				}
				if (tempTable.TableName == "State")
				{
					List<string> list2 = null;
					try
					{
						list2 = Common.DeserializeFromBase64<List<string>>(_default_settings);
					}
					catch
					{
					}
					if (list2 != null)
					{
						foreach (string item3 in list2)
						{
							string[] array8 = item3.Split('/');
							if (array8.Length == 2)
							{
								string text12 = array8[0];
								_ = array8[1];
								string filterExpression8 = "Key = '" + text12 + "'";
								DataRow[] array9 = tempTable.Select(filterExpression8);
								if (array9.Length >= 1 && dataTable3.Select(filterExpression8).Length == 0)
								{
									dataTable3.ImportRow(array9[0]);
								}
							}
						}
					}
				}
				dataSet3.Merge(dataTable3);
				log = log + "Imported table <" + table2.TableName + "> into database.\n";
				continue;
			}
			log = log + "Unrecognized table: " + table2.TableName + "\n";
		}
		if (flag)
		{
			foreach (DataTable table7 in dataSet2.Tables)
			{
				if (table7.TableName.StartsWith("MeterDisplay_") && !dataSet3.Tables.Contains(table7.TableName))
				{
					dataSet3.Merge(table7);
				}
			}
		}
		if (!ignore_merged)
		{
			_merged = true;
		}
		ds = dataSet3.Copy();
		log += "\nImport succeeded.\n";
		return true;
	}

	private static bool getRadioSelectedFromOldRadButton(ref DataTable tempTable, string sRadButtonName)
	{
		bool result = false;
		string filterExpression = "Key = '" + sRadButtonName + "'";
		DataRow[] array = tempTable.Select(filterExpression);
		if (array.Length != 0)
		{
			result = Convert.ToBoolean(array[0]["Value"]);
		}
		return result;
	}

	private static string ValidateImportedDatabase(DataSet oldDB)
	{
		string text = "";
		if (oldDB.HasErrors || oldDB.DataSetName != "Data" || !oldDB.IsInitialized || oldDB.Tables.Count == 0)
		{
			text = text + "Invalid database read. \n\nDataSet characteristics:\nHasErrors=" + oldDB.HasErrors + "\nDataSetName=" + oldDB.DataSetName + "\nIsInitialized=" + oldDB.IsInitialized + "\nNamespace=" + oldDB.Namespace + "\nNumTables=" + oldDB.Tables.Count + "\n";
			foreach (DataTable table in oldDB.Tables)
			{
				text = text + "\nTable: " + table.TableName;
			}
		}
		return text;
	}

	private static DataTable ExpandOldTxProfileTable(DataTable oldTable)
	{
		DataTable dataTable = ds.Tables["TXProfileDef"];
		DataTable dataTable2 = dataTable.Clone();
		DataRow[] array = dataTable.Select("Name = 'Default'");
		if (array.Length != 0)
		{
			DataRow dataRow = array[0];
			{
				foreach (DataRow row in oldTable.Rows)
				{
					DataRow dataRow3 = dataRow;
					foreach (DataColumn column in oldTable.Columns)
					{
						if (ds.Tables["TXProfile"].Columns.Contains(column.ColumnName))
						{
							Type type = row[column.ColumnName].GetType();
							if (dataRow3[column.ColumnName].GetType().FullName == type.FullName)
							{
								dataRow3[column.ColumnName] = row[column.ColumnName];
							}
						}
					}
					if ("Default" != (string)dataRow3["Name"])
					{
						dataTable2.ImportRow(dataRow3);
					}
				}
				return dataTable2;
			}
		}
		return null;
	}

	private static void WriteImportLog(string logFN, string s)
	{
		File.AppendAllText(logFN, s);
	}

	public static bool ImportDatabase(string filename)
	{
		if (!File.Exists(filename))
		{
			return false;
		}
		DataSet dataSet = new DataSet();
		try
		{
			dataSet.ReadXml(filename);
		}
		catch (Exception)
		{
			return false;
		}
		ds = dataSet;
		DataRow[] array = ds.Tables["BandStack"].Select("Mode = 'FMN'");
		for (int i = 0; i < array.Length; i++)
		{
			array[i]["Mode"] = "FM";
		}
		return true;
	}

	public static void UpdateRegion(FRSRegion current_region)
	{
		switch (current_region)
		{
		case FRSRegion.US:
		case FRSRegion.Australia:
			ClearBandText();
			AddRegion2BandText();
			AddBandTextSWB();
			break;
		case FRSRegion.Japan:
			ClearBandText();
			AddJapanBandText160m();
			AddJapanBandText80m();
			AddRegion3BandText60m();
			AddJapanBandTextEmergency();
			AddJapanBandText40m();
			AddRegion3BandText30m();
			AddRegion3BandText20m();
			AddRegion3BandText17m();
			AddRegion3BandText15m();
			AddRegion3BandText12m();
			AddJapanBandText10m();
			AddJapanBandText6m();
			AddRegion3BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.India:
			ClearBandText();
			AddRegionIndiaBandText160m();
			AddRegionIndiaBandText80m();
			AddRegionIndiaBandText40m();
			AddRegion1BandText30m();
			AddRegionIndiaBandText20m();
			AddRegionIndiaBandText17m();
			AddRegionIndiaBandText15m();
			AddRegionIndiaBandText12m();
			AddRegionIndiaBandText10m();
			AddRegionIndiaBandText6m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Spain:
		case FRSRegion.Slovakia:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Europe:
		case FRSRegion.Italy_Plus:
		case FRSRegion.Germany:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddEUBandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Israel:
			ClearBandText();
			AddRegionIsraelBandText160m();
			AddRegionIsraelBandText80m();
			AddRegionIsraelBandText60m();
			AddRegionIsraelBandText40m();
			AddRegionIsraelBandText30m();
			AddRegionIsraelBandText20m();
			AddRegionIsraelBandText17m();
			AddRegionIsraelBandText15m();
			AddRegionIsraelBandText12m();
			AddRegionIsraelBandText10m();
			AddRegionIsraelBandText6m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.UK:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddUK_PlusBandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Norway:
		case FRSRegion.Denmark:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddNorwayBandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Latvia:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddLatviaBandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Bulgaria:
			ClearBandText();
			AddBulgariaBandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddBulgariaBandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Greece:
			ClearBandText();
			AddBulgariaBandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddGreeceBandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Hungary:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddHungaryBandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Netherlands:
			ClearBandText();
			AddNetherlandsBandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.France:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddFranceBandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Russia:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRussiaBandText12m();
			AddRussiaBandText11m();
			AddRegion1BandText10m();
			AddGreeceBandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Sweden:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddSwedenBandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandText4m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Region1:
			ClearBandText();
			AddRegion1BandText160m();
			AddRegion1BandText80m();
			AddRegion1BandText60m();
			AddRegion1BandText40m();
			AddRegion1BandText30m();
			AddRegion1BandText20m();
			AddRegion1BandText17m();
			AddRegion1BandText15m();
			AddRegion1BandText12m();
			AddRegion1BandText10m();
			AddRegion1BandText6m();
			AddRegion1BandTextVHFplus();
			AddBandTextSWB();
			break;
		case FRSRegion.Region2:
			ClearBandText();
			AddBandTextSWB();
			break;
		case FRSRegion.Region3:
			ClearBandText();
			AddBandTextSWB();
			break;
		}
		CheckBandTextValid();
		WriteDB();
	}
}
