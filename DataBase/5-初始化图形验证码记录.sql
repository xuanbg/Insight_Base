use Insight_Base
go

insert SYS_VerifyImage (Type, Name, Path)
select '0', 'mp0', '/image/0.png' union all
select '0', 'mp1', '/image/1.png' union all
select '0', 'mp2', '/image/2.png' union all
select '0', 'mp3', '/image/3.png' union all
select '0', 'mp4', '/image/4.png' union all
select '0', 'mp5', '/image/5.png' union all
select '0', 'mp6', '/image/6.png' union all
select '0', 'mp7', '/image/7.png' union all
select '0', 'mp8', '/image/8.png' union all
select '1', 'bg0', '/image/0.jpg' union all
select '1', 'bg1', '/image/1.jpg' union all
select '1', 'bg2', '/image/2.jpg' union all
select '1', 'bg3', '/image/3.jpg' union all
select '1', 'bg4', '/image/4.jpg' union all
select '1', 'bg5', '/image/5.jpg' union all
select '1', 'bg6', '/image/6.jpg' union all
select '1', 'bg7', '/image/7.jpg' union all
select '1', 'bg8', '/image/8.jpg' union all
select '1', 'bg9', '/image/9.jpg'
