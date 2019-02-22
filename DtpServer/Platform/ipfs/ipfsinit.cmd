set IPFS_PATH=%1
set LIBP2P_FORCE_PNET=1
ipfs.exe init
ipfs bootstrap rm --all
ipfs bootstrap add /dns4/dtp.northeurope.cloudapp.azure.com/tcp/4001/ipfs/QmbQsznVuo4GA9qzWZXQVbonkw89uf4zyRp1iPoyef2dVo