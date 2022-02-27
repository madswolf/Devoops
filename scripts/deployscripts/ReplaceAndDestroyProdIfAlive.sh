if python3 MinitwitAlive.py $1 $2; then
        if python3 reassign_floating_ip.py $1 $2; then
            echo Destroying production droplet $3
            vagrant destroy -f $3
            if python3 rename_droplet.py $1 $3 $2; then
                echo rename of droplet $1 successful
            else 
                echo rename of droplet $1 failed
            fi
        else 
            echo staging droplet $1 not found
            cd ../../aspnet/
            vagrant destroy -f $3
        fi
else
    echo staging droplet $1 not alive
    cd ../../aspnet/
    vagrant destroy -f $3
fi